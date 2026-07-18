using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

public sealed record GetAllWorkOrderQuery(
    int PageIndex,
    int PageSize,
    string? SearchTerm,
    string SortColumn = "CreatedAt",
    string SortDirection = "desc",
    WorkOrderState? State = null,
    Guid? LaborId = null,
    Guid? VehicleId = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? EndDateFrom = null,
    DateTime? EndDateTo = null,
    Spot? Spot = null
)
: ICachedQuery<Result<PaginatedList<WorkOrderListItemDto>>>
{
    public string CacheKey =>
                             $"work-orders:p={PageIndex}:ps={PageSize}" +
                             $":q={SearchTerm ?? "-"}" +
                             $":sort={SortColumn}:{SortDirection}" +
                             $":state={State?.ToString() ?? "-"}" +
                             $":veh={VehicleId?.ToString() ?? "-"}" +
                             $":lab={LaborId?.ToString() ?? "-"}" +
                             $":StartAtFrom={StartDateFrom?.ToString("yyyyMMdd") ?? "-"}" +
                             $":StartAtTo={StartDateTo?.ToString("yyyyMMdd") ?? "-"}" +
                             $":EndAtFrom={EndDateFrom?.ToString("yyyyMMdd") ?? "-"}" +
                             $":EndAtTo={EndDateTo?.ToString("yyyyMMdd") ?? "-"}" +
                             $":spot={Spot?.ToString() ?? "-"}";
    public string[] Tags => ["WorkOrders"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetAllWorkOrderQueryHandler(IAppDbContext context)
: IRequestHandler<GetAllWorkOrderQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle
    (GetAllWorkOrderQuery query, CancellationToken cancellationToken)
    {
        var workOrdersQuery = context.WorkOrders.AsNoTracking()
                                     .Include(n => n.Vehicle).ThenInclude(n => n.VehicleModel).ThenInclude(n => n.VehicleMake).Include(n => n.Vehicle.Customer)
                                     .Include(n => n.Invoice)
                                     .Include(n => n.Labor)
                                     .Include(n => n.RepairTasks).ThenInclude(n => n.Parts).AsQueryable();
        workOrdersQuery = ApplyFilters(workOrdersQuery, query);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            workOrdersQuery = ApplySearchTerm(workOrdersQuery, query.SearchTerm);
        }

        workOrdersQuery = ApplySorting(workOrdersQuery, query.SortColumn, query.SortDirection);

        var count = await workOrdersQuery.CountAsync(cancellationToken);

        var items = await workOrdersQuery
              .Skip((query.PageIndex - 1) * query.PageSize)
              .Take(query.PageSize)
              .Select(wo => new WorkOrderListItemDto
              {
                  WorkOrderId = wo.Id,
                  InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
                  Spot = wo.Spot,
                  StartAtUtc = wo.StartAtUtc,
                  EndAtUtc = wo.EndAtUtc,
                  Vehicle = new VehicleDto(
                                            wo.Vehicle.Id,
                                            wo.Vehicle.VehicleModel.VehicleMake.Make,
                                            wo.Vehicle.VehicleModel.Model,
                                            wo.Vehicle.Year,
                                            wo.Vehicle.LicensePlate),
                  Customer = wo.Vehicle!.Customer!.Name,
                  Labor = wo.Labor != null
                    ? wo.Labor.FirstName + " " + wo.Labor.LastName
                    : null,
                  State = wo.State,
                  RepairTasks = wo.RepairTasks.Select(rt => rt.Name).ToList()
              })
            .ToListAsync(cancellationToken);

        return new PaginatedList<WorkOrderListItemDto>
        {
            Items = items,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = count,
        };
    }

    private static IQueryable<WorkOrder> ApplyFilters(IQueryable<WorkOrder> query, GetAllWorkOrderQuery searchQuery)
    {
        if (searchQuery.State.HasValue)
        {
            query = query.Where(wo => wo.State == searchQuery.State.Value);
        }

        if (searchQuery.VehicleId.HasValue && searchQuery.VehicleId != Guid.Empty)
        {
            query = query.Where(wo => wo.VehicleId == searchQuery.VehicleId.Value);
        }

        if (searchQuery.LaborId.HasValue && searchQuery.LaborId != Guid.Empty)
        {
            query = query.Where(wo => wo.LaborId == searchQuery.LaborId.Value);
        }

        if (searchQuery.StartDateFrom.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc >= searchQuery.StartDateFrom.Value);
        }

        if (searchQuery.StartDateTo.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc <= searchQuery.StartDateTo.Value);
        }

        if (searchQuery.EndDateFrom.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc >= searchQuery.EndDateFrom.Value);
        }

        if (searchQuery.EndDateTo.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc <= searchQuery.EndDateTo.Value);
        }

        if (searchQuery.Spot.HasValue)
        {
            query = query.Where(wo => wo.Spot == searchQuery.Spot.Value);
        }

        return query;
    }

    // private static IQueryable<WorkOrder> ApplySearchTerm(IQueryable<WorkOrder> query, string searchTerm)
    // {
    //     var normalized = searchTerm.Trim().ToLower();

    //     return query.Where(wo =>
    //         (wo.Vehicle != null && (
    //             wo.Vehicle.VehicleModel.VehicleMake.Make.ToLower().Contains(normalized) ||
    //             wo.Vehicle.VehicleModel.Model.ToLower().Contains(normalized) ||
    //             wo.Vehicle.LicensePlate.ToLower().Contains(normalized)
    //         )) ||
    //         (wo.Labor != null && (
    //             wo.Labor.FirstName.ToLower().Contains(normalized) ||
    //             wo.Labor.LastName.ToLower().Contains(normalized) ||
    //             (wo.Labor.FirstName + " " + wo.Labor.LastName).ToLower().Contains(normalized)
    //         )) ||
    //         wo.RepairTasks.Any(rt =>
    //             rt.Name.ToLower().Contains(normalized)) ||
    //         wo.Id.ToString().ToLower().Contains(normalized));
    // }

    private static IQueryable<WorkOrder> ApplySearchTerm(IQueryable<WorkOrder> query, string searchTerm)
    {
        var normalized = searchTerm.CapitalizeFirstLetter();

        return query.Where(wo =>
            (wo.Vehicle != null && (
                EF.Functions.Like(wo.Vehicle.VehicleModel.VehicleMake.Make, $"%{normalized}%") ||
                EF.Functions.Like(wo.Vehicle.VehicleModel.Model, $"%{normalized}%") ||
                EF.Functions.Like(wo.Vehicle.LicensePlate, $"%{normalized}%")
            )) ||
            (wo.Labor != null && (
                EF.Functions.Like(wo.Labor.FirstName, $"%{normalized}%") ||
                EF.Functions.Like(wo.Labor.LastName, $"%{normalized}%") ||
                EF.Functions.Like(wo.Labor.FirstName + " " + wo.Labor.LastName, $"%{normalized}%")
            )) ||
            wo.RepairTasks.Any(rt =>
                EF.Functions.Like(rt.Name, $"%{normalized}%")) ||
            EF.Functions.Like(wo.Id.ToString(), $"%{normalized}%"));
    }

    private static IQueryable<WorkOrder> ApplySorting(IQueryable<WorkOrder> query, string sortColumn, string sortDirection)
    {
        var isDescending = sortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(wo => wo.CreatedAtUtc) : query.OrderBy(wo => wo.CreatedAtUtc),
            "updatedat" => isDescending ? query.OrderByDescending(wo => wo.LastModifiedUtc) : query.OrderBy(wo => wo.LastModifiedUtc),
            "startat" => isDescending ? query.OrderByDescending(wo => wo.StartAtUtc) : query.OrderBy(wo => wo.StartAtUtc),
            "endat" => isDescending ? query.OrderByDescending(wo => wo.EndAtUtc) : query.OrderBy(wo => wo.EndAtUtc),
            "state" => isDescending ? query.OrderByDescending(wo => wo.State) : query.OrderBy(wo => wo.State),
            "spot" => isDescending ? query.OrderByDescending(wo => wo.Spot) : query.OrderBy(wo => wo.Spot),
            "total" => isDescending ? query.OrderByDescending(wo => wo.Total) : query.OrderBy(wo => wo.Total),
            "vehicleid" => isDescending ? query.OrderByDescending(wo => wo.VehicleId) : query.OrderBy(wo => wo.VehicleId),
            "laborid" => isDescending ? query.OrderByDescending(wo => wo.LaborId) : query.OrderBy(wo => wo.LaborId),
            _ => query.OrderByDescending(wo => wo.CreatedAtUtc) // Default sorting
        };
    }


}

