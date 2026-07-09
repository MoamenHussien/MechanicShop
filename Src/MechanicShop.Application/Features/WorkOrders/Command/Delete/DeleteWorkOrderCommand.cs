using System.Data;
using System.Net.Mime;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record DeleteWorkOrderCommand(Guid id) : IRequest<Result<Deleted>>;

public class DeleteWorkOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
{
    public DeleteWorkOrderCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Work Order");
    }
}

public class DeleteWorkOrderCommandHandler(ILogger<DeleteCustomerCommandHandler> logger, IAppDbContext context, HybridCache cache,IIdentityService identity,IUser user)
: IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        if(await identity.IsInRoleAsync(user.Id!.Value,Role.Labor.ToString()))
        {
            logger.LogWarning("Delete Work Order: (Forbidden) , This User {UserId} is not allowed to Delete This Work Order", user.Id);
            return ApplicationErrors.NotAllowed;
        }

        var WorkOrder = await context.WorkOrders.FindAsync(request.id, cancellationToken);

        if (WorkOrder is null)
        {
            logger.LogWarning("The Work Order Not Found For This Id {id}", request.id);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }
        
        var IsDeletedState =WorkOrder.MarkAsDeleted();
        
        if (IsDeletedState.IsError)
        {
            logger.LogWarning("Cannot delete this work order id :{id} because its current status is '{state}', which does not allow deletion", request.id, WorkOrder.State);
            return IsDeletedState.Errors;
        }
        context.WorkOrders.Remove(WorkOrder);

        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync("WorkOrders",cancellationToken);

        logger.LogInformation("WorkOrder with Id '{WorkOrderId}' was successfully removed, and cache tag 'WorkOrders' was cleared",request.id);

        return Result.Deleted;
    }

}