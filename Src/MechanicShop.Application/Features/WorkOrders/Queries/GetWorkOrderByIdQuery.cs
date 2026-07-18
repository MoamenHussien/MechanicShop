using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetWorkOrderByIdQuery(Guid id) : ICachedQuery<Result<WorkOrderDto>>
{
    public string CacheKey => $"Work-Order:{id}";

    public string[] Tags => ["WorkOrders"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(n => n.id).IdRequired("Work Order");
    }
}

public class GetWorkOrderByIdQueryHandler(ILogger<GetWorkOrderByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {

        // var WorkOrder = await context.WorkOrders.AsNoTracking()
        //                                   .Include(n=>n.Vehicle).ThenInclude(n=>n.VehicleModel).ThenInclude(n=>n.VehicleMake).Include(n=>n.Vehicle.Customer)
        //                                   .Include(n=>n.Invoice)
        //                                   .Include(n=>n.Labor)
        //                                   .Include(n=>n.RepairTasks).ThenInclude(n=>n.Parts).FirstOrDefaultAsync(n=>n.Id==request.id,cancellationToken);
        var workOrder = await context.WorkOrders
                .AsNoTracking()
                .Where(w => w.Id == request.id)
               .Select(w => new WorkOrderDto
               {
                   WorkOrderId = w.Id,
                   InvoiceId = w.Invoice != null ? w.Invoice.Id : null,
                   Spot = w.Spot,

                   Vehicle = new VehicleDto(
                        w.Vehicle.Id,
                        w.Vehicle.VehicleModel.VehicleMake.Make,
                        w.Vehicle.VehicleModel.Model,
                        w.Vehicle.Year,
                        w.Vehicle.LicensePlate
                        ),

                   StartAtUtc = w.StartAtUtc,
                   EndAtUtc = w.EndAtUtc,

                   RepairTasks = w.RepairTasks.Select(rt => new RepairTaskDto
                   {
                       RepairTaskId = rt.Id,
                       Name = rt.Name,
                       EstimatedDurationInMins = rt.EstimatedDuration,
                       LaborCost = rt.LaborCost,

                       Parts = rt.Parts
                           .Select(p => new PartDto(
                               p.Id,
                               p.Name,
                               p.Costs,
                               p.Quantity))
                           .ToList()

                   }).ToList(),

                   Labor = new LaborDto( w.Labor.Id, w.Labor.FullName),

                   State = w.State,

                   CreatedAt = w.CreatedAtUtc
               })
                .FirstOrDefaultAsync(cancellationToken);


        if (workOrder is null)
        {
            logger.LogWarning("WorkOrder with id {WorkOrderId} was not found", request.id);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        logger.LogInformation("Getting Work Order To This Id Is Successfully : {id}", request.id);

        return workOrder;
    }
}
