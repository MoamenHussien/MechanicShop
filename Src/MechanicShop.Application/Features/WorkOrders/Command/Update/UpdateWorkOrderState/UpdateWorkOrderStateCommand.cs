using System.ComponentModel;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Xml;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
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

public class UpdateWorkOrderStateCommandHandler(ILogger<UpdateWorkOrderStateCommandHandler> logger, IAppDbContext context, TimeProvider time, ICacheInvalidator cacheInvalidator)
: IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
    {
        var WorkOrder = await context.WorkOrders.FindAsync([request.WordOrderId], cancellationToken);
        if (WorkOrder is null)
        {
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        if (WorkOrder.State == request.NewState)
        {
            return Result.Updated;
        }

        // var IsLaborUser = await identity.IsInRoleAsync(user.Id!.Value, Role.Labor.ToString());

        // if (IsLaborUser && WorkOrder.LaborId != user.Id)
        //     {
        //     logger.LogWarning("Update State Failed: User '{UserId}' is not assigned to WorkOrder '{WorkOrderId}'", user.Id, WorkOrder.Id);

        //     return ApplicationErrors.NotAllowed;
        // }

        var utcNow = time.GetUtcNow().UtcDateTime;

        var result = request.NewState switch
        {
            WorkOrderState.InProgress => WorkOrder.StartAtUtc <= utcNow ? WorkOrder.MarkAsInProgress() : ApplicationErrors.WorkOrderStartTimeNotComing(WorkOrder.StartAtUtc),
            WorkOrderState.Completed => WorkOrder.StartAtUtc <= utcNow ? WorkOrder.MarkAsCompleted() : ApplicationErrors.WorkOrderStartTimeNotComing(WorkOrder.StartAtUtc),
            WorkOrderState.Cancelled => WorkOrder.MarkAsCancelled(),
            _ => Error.Validation("Invalid state")
        };

        if (result.IsError)
        {
            logger.LogError("Failed to update status: {Error}", result.TopError.Description);
            return result.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        logger.LogInformation("Successfully updated WorkOrder Id: {WorkOrderId} State to: {NewState} , And Remove Cache Tag 'WorkOrders' ", WorkOrder.Id, request.NewState.ToString());
        return Result.Updated;
    }
}
