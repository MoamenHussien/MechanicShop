using System.Data;
using System.Net.Mime;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
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

public class DeleteWorkOrderCommandHandler(ILogger<DeleteCustomerCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // if(await identity.IsInRoleAsync(user.Id!.Value,Role.Labor.ToString()))
        // {
        //     logger.LogWarning("Delete Work Order: (Forbidden) , This User {UserId} is not allowed to Delete This Work Order", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }
        var workOrder = await context.WorkOrders.FindAsync([request.id], cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("The Work Order Not Found For This Id {id}", request.id);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        var isDeletedState = workOrder.MarkAsDeleted();

        if (isDeletedState.IsError)
        {
            logger.LogWarning("Cannot delete this work order id :{id} because its current status is '{state}', which does not allow deletion", request.id, workOrder.State);
            return isDeletedState.Errors;
        }

        context.WorkOrders.Remove(workOrder);

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        logger.LogInformation("WorkOrder with Id '{WorkOrderId}' was successfully removed, and cache tag 'WorkOrders' was cleared", request.id);

        return Result.Deleted;
    }
}
