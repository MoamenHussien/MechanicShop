using System.ComponentModel;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Xml;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record UpdateWorkOrderStateCommand(Guid WordOrderId, WorkOrderState NewState) : IRequest<Result<Updated>>;

public class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(n => n.WordOrderId).IdRequired("Work Order");
        RuleFor(n => n.NewState).IsInEnum().WithMessage("Enter Valid New Work Order State");
    }
}

public class UpdateWorkOrderStateCommandHandler(ILogger<UpdateWorkOrderStateCommandHandler> logger, IAppDbContext context, IUser user, IIdentityService identity,TimeProvider time,HybridCache cache)
: IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
    {
        var WorkOrder = await context.WorkOrders.FindAsync([request.WordOrderId], cancellationToken);
        if (WorkOrder is null)
        {
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        if(WorkOrder.State == request.NewState)
        {
           return  ApplicationErrors.NothingIsChanged;
        }

         if (await identity.IsInRoleAsync(user.Id!.Value, Role.Labor.ToString()))
        {
            if ( WorkOrder.LaborId != user.Id.Value)
            {
                logger.LogWarning("State change forbidden: User {UserId} attempted to modify WorkOrder {WorkOrderId} not assigned to them",user.Id, WorkOrder.Id);
                 return ApplicationErrors.NotAllowedToProcessWorkOrder;
            }  
        }

        var TimeNow = time.GetUtcNow();

        var result = request.NewState switch
        {
            WorkOrderState.InProgress => WorkOrder.StartAtUtc <= TimeNow ? WorkOrder.MarkAsInProgress() : ApplicationErrors.WorkOrderStartTimeNotComing(WorkOrder.StartAtUtc),
            WorkOrderState.Completed  =>  WorkOrder.StartAtUtc <= TimeNow ? WorkOrder.MarkAsCompleted() : ApplicationErrors.WorkOrderStartTimeNotComing(WorkOrder.StartAtUtc),
            WorkOrderState.Cancelled  => WorkOrder.MarkAsCancelled(),
            _=> Error.Validation("Invalid state")
        };

        if (result.IsError)
        {
            logger.LogError("Failed to update status: {Error}", result.TopError.Description);
            return result.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync("WorkOrders",cancellationToken);

        logger.LogInformation("Successfully updated WorkOrder Id: {WorkOrderId} State to: {NewState} , And Remove Cache Tag 'WorkOrders' ", WorkOrder.Id, request.NewState.ToString());
        return Result.Updated;
    }
}