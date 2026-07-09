using System.Numerics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record GetWorkOrderStatsQuery(DateOnly Date) : IRequest<Result<TodayWorkOrderStatsDto>>;

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
        var StartDayTimeUtc = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var EndDayTimeUtc = StartDayTimeUtc.AddDays(1);

        var WorkOrders = context.WorkOrders.AsNoTracking().Where(n => n.StartAtUtc >= StartDayTimeUtc && n.StartAtUtc < EndDayTimeUtc)
                                                          .Include(n=>n.RepairTasks).ThenInclude(n=>n.Parts)
                                                          .Include(n=>n.Vehicle)
                                                          .Include(n=>n.Invoice);

        var WorkOrderCount = await WorkOrders.CountAsync(cancellationToken);

        if (WorkOrderCount == 0)
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
                UniqueCustomers = 0
            };

        }


        var result = await WorkOrders.ToListAsync(cancellationToken);

        var TotalRevenue = result.Sum(n=>n.Invoice?.Total??0) ;
        var totalPartCost = result.Where(n=>n.Invoice != null).Sum(n=>n.TotalPartsCost);
        var totalLaborCost = result.Where(n=>n.Invoice != null).Sum(n=>n.TotalLaborCost);
        var uniqueVehicles = result.Select(n=>n.VehicleId).Distinct().Count();
        var uniqueCustomers = result.Select(n=>n.Vehicle.CustomerId).Distinct().Count();
        var netProfit = TotalRevenue - totalLaborCost - totalPartCost;


        return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = WorkOrderCount,
            Scheduled =    result.Count(n => n.State == WorkOrderState.Scheduled),
            InProgress =   result.Count(n => n.State == WorkOrderState.InProgress),
            Completed =    result.Count(n => n.State == WorkOrderState.Completed),
            Cancelled =    result.Count(n => n.State == WorkOrderState.Cancelled),
            TotalRevenue =  TotalRevenue,
            TotalPartsCost = totalPartCost  ,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqueVehicles,
            UniqueCustomers =uniqueCustomers ,
            NetProfit = netProfit,
            ProfitMargin = TotalRevenue > 0 ? (netProfit / TotalRevenue) * 100 : 0 ,
            CompletionRate = WorkOrderCount > 0 ? ((decimal) result.Count(n => n.State == WorkOrderState.Completed) / WorkOrderCount ) * 100 : 0 ,
            AverageRevenuePerOrder = WorkOrderCount > 0 ? (TotalRevenue / WorkOrderCount) : 0,
            OrdersPerVehicle =  uniqueVehicles > 0 ? (decimal)WorkOrderCount / uniqueVehicles : 0,
            PartsCostRatio = TotalRevenue > 0 ? (totalPartCost / TotalRevenue) * 100 : 0,
            LaborCostRatio =  TotalRevenue > 0 ? (totalLaborCost / TotalRevenue) * 100 : 0,
            CancellationRate = WorkOrderCount > 0 ? ( (decimal) result.Count(n => n.State == WorkOrderState.Cancelled) / WorkOrderCount ) * 100 : 0
        };

    }


}




