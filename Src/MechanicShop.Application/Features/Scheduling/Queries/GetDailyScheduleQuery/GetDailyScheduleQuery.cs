using System.Linq.Expressions;
using MechanicShop.Application.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

public sealed record GetDailyScheduleQuery(DateOnly ScheduleDate, TimeZoneInfo TimeZone, Guid? LaborId = null) : ICachedQuery<Result<ScheduleDto>>
{
    public string CacheKey => $"date:{ScheduleDate:yyyyMMdd}-labor:{LaborId?.ToString() ?? "-"}";

    public string[] Tags => [CacheTags.WorkOrders, CacheTags.Schedules];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetDailyScheduleQueryHandler(IAppDbContext context, TimeProvider time) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
    {
        var localStartDayConst = request.ScheduleDate.ToDateTime(TimeOnly.MinValue);
        var localEndDayConst = localStartDayConst.AddDays(1);

        var utcStartDay = localStartDayConst.ToUtc(request.TimeZone);
        var utcEndDay = localEndDayConst.ToUtc(request.TimeZone);

        var workOrders = await context.WorkOrders.AsNoTracking().Where(n => n.StartAtUtc < utcEndDay && n.EndAtUtc > utcStartDay &&
                                                                      (request.LaborId == null || n.LaborId == request.LaborId))
                                                                .Include(n => n.Vehicle).ThenInclude(n => n.VehicleModel).ThenInclude(n => n.VehicleMake)
                                                                .Include(n => n.Labor)
                                                                .Include(n => n.RepairTasks).ToListAsync(cancellationToken);

        var localTimeNow = TimeZoneInfo.ConvertTime(time.GetUtcNow(), request.TimeZone);

        var scheduleDto = new ScheduleDto
        {
            OnDate = request.ScheduleDate,
            EndOfDay = localEndDayConst < localTimeNow,
            Spots = [],
        };

        foreach (var spotEn in Enum.GetValues<Spot>())
        {
            var spotDto = new SpotDto();
            var hash = new HashSet<Guid>();

            var workOrdersInThisSpot = workOrders.Where(n => n.Spot == spotEn).OrderBy(n => n.StartAtUtc).ToList();

            var startRangeLocal = localStartDayConst;
            var endRangeLocal = startRangeLocal + TimeSpan.FromMinutes(15);

            spotDto.Spot = spotEn;

            while (localEndDayConst > startRangeLocal)
            {
                var startRangeUtc = startRangeLocal.ToUtc(request.TimeZone);
                var endRangeUtc = endRangeLocal.ToUtc(request.TimeZone);

                var workOrderInThisTime = workOrdersInThisSpot.FirstOrDefault(n => n.StartAtUtc < endRangeUtc && n.EndAtUtc > startRangeUtc);

                if (workOrderInThisTime is not null)
                {
                    if (hash.Add(workOrderInThisTime.Id))
                    {
                        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(workOrderInThisTime.StartAtUtc.DateTime, request.TimeZone);
                        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(workOrderInThisTime.EndAtUtc.DateTime, request.TimeZone);
                        spotDto.Slots.Add(new AvailabilitySlotDto
                        {
                            WorkOrderId = workOrderInThisTime.Id,
                            Spot = spotEn,
                            StartAt = new DateTimeOffset(startLocal, request.TimeZone.GetUtcOffset(startLocal)),
                            EndAt = new DateTimeOffset(endLocal, request.TimeZone.GetUtcOffset(endLocal)),
                            Vehicle = workOrderInThisTime.Vehicle?.VehicleModel?.VehicleMake?.Make + " | " + workOrderInThisTime.Vehicle?.LicensePlate,
                            Labor = workOrderInThisTime.Labor.ToDto(),
                            IsOccupied = true,
                            IsAvailable = false,
                            WorkOrderLocked = workOrderInThisTime.IsEditable == false,
                            State = workOrderInThisTime.State,
                            RepairTasks = workOrderInThisTime.RepairTasks.ToDto().ToArray(),
                        });
                    }
                }
                else
                {
                    spotDto.Slots.Add(new AvailabilitySlotDto
                    {
                        WorkOrderId = null,
                        Spot = spotEn,
                        StartAt = new DateTimeOffset(startRangeLocal, request.TimeZone.GetUtcOffset(startRangeLocal)),
                        EndAt = new DateTimeOffset(endRangeLocal, request.TimeZone.GetUtcOffset(endRangeLocal)),
                        Vehicle = null,
                        Labor = null,
                        IsOccupied = false,
                        IsAvailable = localTimeNow <= startRangeLocal,
                        WorkOrderLocked = false,
                        State = null,
                        RepairTasks = null,
                    });
                }

                startRangeLocal = endRangeLocal;
                endRangeLocal = startRangeLocal + TimeSpan.FromMinutes(15);
            }

            scheduleDto.Spots.Add(spotDto);
        }

        return scheduleDto;
    }
}
