using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetWorkOrderByIdQuery(Guid id) : ICachedQuery<Result<WorkOrderDto>>{
    public string CacheKey => $"Work-Order:{id}";

    public string[] Tags => ["WorkOrders"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(n=>n.id).IdRequired("Work Order");
    }
}

public class GetWorkOrderByIdQueryHandler(ILogger<GetWorkOrderByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var WorkOrder = await context.WorkOrders.AsNoTracking()
                                          .Include(n=>n.Vehicle).ThenInclude(n=>n.VehicleModel).ThenInclude(n=>n.VehicleMake).Include(n=>n.Vehicle.Customer)
                                          .Include(n=>n.Invoice)
                                          .Include(n=>n.Labor)
                                          .Include(n=>n.RepairTasks).ThenInclude(n=>n.Parts).FirstOrDefaultAsync(n=>n.Id==request.id,cancellationToken);
        if(WorkOrder is null)
        {
            logger.LogWarning("WorkOrder with id {WorkOrderId} was not found", request.id);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        logger.LogInformation("Getting Work Order To This Id Is Successfully : {id}",request.id);

        return WorkOrder.ToDto();
    }
}
