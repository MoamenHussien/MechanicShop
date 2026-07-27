using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

public sealed record UpdateWorkOrderRepairTasksCommand(Guid WorkOrderid, Guid[] RepairTasksIds) : IRequest<Result<Updated>>;

public class UpdateWorkOrderRepairTasksCommandValidator : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
    public UpdateWorkOrderRepairTasksCommandValidator()
    {
        RuleFor(n => n.WorkOrderid).IdRequired("Work Order");
        RuleFor(n => n.RepairTasksIds).NotNull().WithMessage("Repair Tasks Is Required").Must(n => n != null && n.Count() > 0).WithMessage("You Must Select At Least One Repair Task");
    }
}

public class UpdateWorkOrderRepairTasksCommandHandler(ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger, IIdentityService identity, IUser user, IAppDbContext context, ICacheInvalidator cacheInvalidator, IWorkOrderPolicy policy)
: IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
    {
        if (await identity.IsInRoleAsync(user.Id!.Value, Role.Labor.ToString()))
        {
            logger.LogWarning("Update Work Order Repair Tasks: (Forbidden) , This User {UserId} is not allowed to Update Repair Tasks", user.Id);
            return ApplicationErrors.NotAllowed;
        }

        var WorkOrder = await context.WorkOrders.Include(n => n.RepairTasks).FirstOrDefaultAsync(n => n.Id == request.WorkOrderid,cancellationToken);

        if (WorkOrder is null)
        {
            logger.LogWarning("WorkOrder '{WorkOrderId}' not found for updating repair tasks", request.WorkOrderid);

            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        var currentIds = WorkOrder.RepairTasks.Select(n => n.Id).ToHashSet();
        var newIds = request.RepairTasksIds.ToHashSet();

        if (currentIds.SetEquals(newIds))
        {
            logger.LogInformation( "No changes detected for WorkOrder {WorkOrderId}",request.WorkOrderid);
            return Result.Updated;
        }

        var RepairTasks = await context.RepairTasks.Where(n => newIds.Contains(n.Id)).ToListAsync(cancellationToken);

        if (!RepairTasks.Any())
        {
            logger.LogError("Not Found Any Repair Tasks For These Ids : {ids}", string.Join(" , ", request.RepairTasksIds));
            return ApplicationErrors.NotFoundAnyRepairTasks;
        }

        if (RepairTasks.Count != request.RepairTasksIds.Length)
        {
            var notfound = request.RepairTasksIds.Except(RepairTasks.Select(n => n.Id));
            logger.LogError("Cant Find Some Selected Repair Tasks , The Ids For Repair Tasks Not Found : {ids}", string.Join(" , ", notfound));
            return ApplicationErrors.SomeRepairTaskIdsNotfound;
        }

        var TotalDurations = TimeSpan.FromMinutes(RepairTasks.Sum(n => (int)n.EstimatedDuration));

        var NewEndAt = WorkOrder.StartAtUtc + TotalDurations;

        if (policy.IsOutsideOperatingHours(WorkOrder.StartAtUtc, TotalDurations))
        {
            logger.LogWarning("Updated repair tasks duration ({StartAt} ? {EndAt}) exceeds operating hours for WorkOrder '{WorkOrderId}'",
                WorkOrder.StartAtUtc, NewEndAt, WorkOrder.Id);
            return ApplicationErrors.WorkOrderOutsideOperatingHour(WorkOrder.StartAtUtc, NewEndAt);
        }

        var MinimumRequirementResult = policy.ValidateMinimumRequirement(WorkOrder.StartAtUtc, NewEndAt);
        if (MinimumRequirementResult.IsError)
        {
            logger.LogWarning("Updated repair tasks fail minimum requirement check for WorkOrder '{WorkOrderId}': {Error}",
                WorkOrder.Id, MinimumRequirementResult.TopError.Description);
            return MinimumRequirementResult.Errors;
        }

        if (await policy.IsLaborOccupiedDuringRange(WorkOrder.StartAtUtc, NewEndAt, WorkOrder.LaborId, WorkOrder.Id,cancellationToken))
        {
            logger.LogError("Labor with Id '{LaborId}' is occupied during the new calculated duration.", WorkOrder.LaborId);
            return ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime;
        }

        var isAvailable = await policy.CheckSpotAvailabilityAsync(WorkOrder.StartAtUtc, NewEndAt, WorkOrder.Spot, WorkOrder.Id, cancellationToken);

        if (isAvailable.IsError)
        {
            logger.LogError("Spot '{Spot}' is already occupied during the new calculated duration.", WorkOrder.Spot.ToString());
            return ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot;
        }

        var RemoveAndInsertedState = WorkOrder.RemoveAndInsertRepairTasks(RepairTasks);
        if (RemoveAndInsertedState.IsError)
        {
            return RemoveAndInsertedState.Errors;
        }

        var updateTimeState = WorkOrder.UpdateTiming(WorkOrder.StartAtUtc, NewEndAt);
        if (updateTimeState.IsError)
        {
            return updateTimeState.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        logger.LogInformation("Successfully updated Repair Tasks for WorkOrder Id: {WorkOrderId}. New Duration: {Duration} mins, New EndAt: {EndAt} , And Remove Cache Tag 'WorkOrders' ",
            WorkOrder.Id, TotalDurations, WorkOrder.EndAtUtc);

        return Result.Updated;

    }
}