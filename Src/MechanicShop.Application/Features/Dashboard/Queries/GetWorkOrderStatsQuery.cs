using System.Numerics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record GetWorkOrderStatsQuery(DateOnly Date, TimeZoneInfo? TimeZone = null) : IRequest<Result<TodayWorkOrderStatsDto>>;

public class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
{
    public GetWorkOrderStatsQueryValidator()
    {
        RuleFor(request => request.Date)
            .NotEmpty()
            .WithErrorCode("Date_Is_Required")
            .WithMessage("Date is required.");
    }
}

public class GetWorkOrderStatsQueryHandler(IAppDbContext context) : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var localStartDayConst = request.Date.ToDateTime(TimeOnly.MinValue);
        var localEndDayConst = localStartDayConst.AddDays(1);

        var timeZone = request.TimeZone ?? TimeZoneInfo.Local;

        var startDayTimeUtc = localStartDayConst.ToUtc(timeZone);
        var endDayTimeUtc = localEndDayConst.ToUtc(timeZone);

        var workOrders = context.WorkOrders.AsNoTracking().Where(n => n.StartAtUtc >= startDayTimeUtc && n.StartAtUtc < endDayTimeUtc)
                                                          .Include(n => n.RepairTasks).ThenInclude(n => n.Parts)
                                                          .Include(n => n.Vehicle)
                                                          .Include(n => n.Invoice);

        var workOrderCount = await workOrders.CountAsync(cancellationToken);

        if (workOrderCount == 0)
        {
            return new TodayWorkOrderStatsDto
            {
                Date = request.Date,
                Total = 0,
                Scheduled = 0,
                InProgress = 0,
                Completed = 0,
                Cancelled = 0,
                TotalRevenue = 0,
                TotalPartsCost = 0,
                TotalLaborCost = 0,
                UniqueVehicles = 0,
                UniqueCustomers = 0,
            };
        }

        var result = await workOrders.ToListAsync(cancellationToken);

        var totalRevenue = result.Sum(n => n.Invoice?.Total ?? 0);
        var totalPartCost = result.Where(n => n.Invoice != null).Sum(n => n.TotalPartsCost);
        var totalLaborCost = result.Where(n => n.Invoice != null).Sum(n => n.TotalLaborCost);
        var uniqueVehicles = result.Select(n => n.VehicleId).Distinct().Count();
        var uniqueCustomers = result.Select(n => n.Vehicle.CustomerId).Distinct().Count();
        var netProfit = totalRevenue - totalLaborCost - totalPartCost;

        return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = workOrderCount,
            Scheduled = result.Count(n => n.State == WorkOrderState.Scheduled),
            InProgress = result.Count(n => n.State == WorkOrderState.InProgress),
            Completed = result.Count(n => n.State == WorkOrderState.Completed),
            Cancelled = result.Count(n => n.State == WorkOrderState.Cancelled),
            TotalRevenue = totalRevenue,
            TotalPartsCost = totalPartCost,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqueVehicles,
            UniqueCustomers = uniqueCustomers,
            NetProfit = netProfit,
            ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
            CompletionRate = workOrderCount > 0 ? ((decimal)result.Count(n => n.State == WorkOrderState.Completed) / workOrderCount) * 100 : 0,
            AverageRevenuePerOrder = workOrderCount > 0 ? (totalRevenue / workOrderCount) : 0,
            OrdersPerVehicle = uniqueVehicles > 0 ? (decimal)workOrderCount / uniqueVehicles : 0,
            PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
            LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
            CancellationRate = workOrderCount > 0 ? ((decimal)result.Count(n => n.State == WorkOrderState.Cancelled) / workOrderCount) * 100 : 0,
        };
    }
}
