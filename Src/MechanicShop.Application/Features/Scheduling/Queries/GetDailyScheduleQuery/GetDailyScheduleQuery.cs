using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

public sealed record GetDailyScheduleQuery(DateOnly ScheduleDate, TimeZoneInfo TimeZone, Guid? LaborId = null) : ICachedQuery<Result<ScheduleDto>>
{
    public string CacheKey => $"date:{ScheduleDate:yyyyMMdd}-labor:{LaborId?.ToString() ?? "-"}";

    public string[] Tags => ["WorkOrders"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetDailyScheduleQueryHandler(IAppDbContext context, TimeProvider time) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
    {
        var LocalStartDayConst = request.ScheduleDate.ToDateTime(TimeOnly.MinValue);
        var LocalEndDayConst = LocalStartDayConst.AddDays(1);

        var UtcStartDay = LocalStartDayConst.ToUtc(request.TimeZone);
        var UtcEndDay = LocalEndDayConst.ToUtc(request.TimeZone);

        var WorkOrders = await context.WorkOrders.AsNoTracking().Where(n => n.StartAtUtc < UtcEndDay && n.EndAtUtc > UtcStartDay &&
                                                                      (request.LaborId == null || n.LaborId == request.LaborId))
                                                                .Include(n => n.Vehicle).ThenInclude(n => n.VehicleModel).ThenInclude(n=>n.VehicleMake)
                                                                .Include(n => n.Labor)
                                                                .Include(n => n.RepairTasks).ToListAsync(cancellationToken);

         var LocalTimeNow = TimeZoneInfo.ConvertTime(time.GetUtcNow(), request.TimeZone);


        var ScheduleDto = new ScheduleDto
        {
            OnDate = request.ScheduleDate,
            EndOfDay = LocalEndDayConst < LocalTimeNow,
            Spots=[]
        };


        foreach (var spotEn in Enum.GetValues<Spot>())
        {
            var SpotDto = new SpotDto();
            var Hash = new HashSet<Guid>();

            var WorkOrdersInThisSpot = WorkOrders.Where(n => n.Spot == spotEn).OrderBy(n => n.StartAtUtc).ToList();

            var startRangeLocal = LocalStartDayConst;
            var EndRangeLocal = startRangeLocal + TimeSpan.FromMinutes(15);

            SpotDto.Spot = spotEn;

            while (LocalEndDayConst > startRangeLocal)
            {
                var StartRangeUtc = startRangeLocal.ToUtc(request.TimeZone);
                var EndRangeUtc = EndRangeLocal.ToUtc(request.TimeZone);

                var WorkOrderInThisTime = WorkOrdersInThisSpot.FirstOrDefault(n => n.StartAtUtc < EndRangeUtc && n.EndAtUtc > StartRangeUtc);

                if (WorkOrderInThisTime is not null)
                {
                    if (Hash.Add(WorkOrderInThisTime.Id))
                    {
                        SpotDto.Slots.Add(new AvailabilitySlotDto
                        {
                            WorkOrderId = WorkOrderInThisTime.Id,
                            Spot = spotEn,
                            StartAt = WorkOrderInThisTime.StartAtUtc,
                            EndAt = WorkOrderInThisTime.EndAtUtc,
                            Vehicle = WorkOrderInThisTime.Vehicle?.VehicleModel?.VehicleMake?.Make +" | "+WorkOrderInThisTime.Vehicle?.LicensePlate,
                            Labor = WorkOrderInThisTime.Labor.ToDto(),
                            IsOccupied = true,
                            IsAvailable = false,
                            WorkOrderLocked = WorkOrderInThisTime.IsEditable == false,
                            State = WorkOrderInThisTime.State,
                            RepairTasks = WorkOrderInThisTime.RepairTasks.ToDto().ToArray()
                        });
                    }
                }
                else
                {
                    SpotDto.Slots.Add(new AvailabilitySlotDto
                    {
                        WorkOrderId = null,
                        Spot = spotEn,
                        StartAt = StartRangeUtc,
                        EndAt = EndRangeUtc,
                        Vehicle = null,
                        Labor = null,
                        IsOccupied = false,
                        IsAvailable = LocalTimeNow <= startRangeLocal,
                        WorkOrderLocked = false,
                        State = null,
                        RepairTasks = null
                    });
                }

                startRangeLocal = EndRangeLocal;
                EndRangeLocal = startRangeLocal + TimeSpan.FromMinutes(15);
            }

            ScheduleDto.Spots.Add(SpotDto);

        }

        return ScheduleDto;

    }
}