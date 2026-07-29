using System.Security.AccessControl;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

public sealed record RelocateWorkOrderCommand(Guid WorkOrderId, DateTimeOffset NewStartDateTimeUtc, Spot NewSpot) : IRequest<Result<Updated>>;

public class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkOrderCommandValidator()
    {
        RuleFor(n => n.WorkOrderId).IdRequired("Work Order");
        RuleFor(n => n.NewStartDateTimeUtc).NotEmpty().WithMessage("The New Work Order Date Is Required").Must(n => n > DateTimeOffset.UtcNow).WithMessage("The New Start Work Order Date Must Be Greater Than DateTimeOffSetUtcNow");
        RuleFor(n => n.NewSpot).IsInEnum().WithMessage("You Must Enter Valid New Spot");
    }
}

public class RelocateWorkOrderCommandHandler(ILogger<RelocateWorkOrderCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator, IUser user, IIdentityService identity, IWorkOrderPolicy policy)
: IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        if (await identity.IsInRoleAsync(user.Id!.Value, Role.Labor.ToString()))
        {
            logger.LogWarning("Relocate WorkOrder forbidden. User {UserId} belongs to Labor role", user.Id);
            return ApplicationErrors.NotAllowed;
        }

        var WorkOrder = await context.WorkOrders.FirstOrDefaultAsync(n => n.Id == request.WorkOrderId, cancellationToken);

        if (WorkOrder is null)
        {
            logger.LogWarning("Relocate Work Order Failed: WorkOrder with Id '{WorkOrderId}' not found.", request.WorkOrderId);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        var TotalDurations = WorkOrder.EndAtUtc.Subtract(WorkOrder.StartAtUtc);

        var newEndAt = request.NewStartDateTimeUtc.Add(TotalDurations);

        if (WorkOrder.StartAtUtc == request.NewStartDateTimeUtc && WorkOrder.EndAtUtc == newEndAt)
        {
            return ApplicationErrors.NothingIsChanged;
        }

        if (policy.IsOutsideOperatingHours(request.NewStartDateTimeUtc, TotalDurations))
        {
            logger.LogWarning("Relocate Work Order Failed: The requested time ({StartAt} - {EndAt}) is outside store operating hours.", request.NewStartDateTimeUtc, newEndAt);
            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.NewStartDateTimeUtc, newEndAt);
        }

        if (await policy.IsVehicleAlreadyScheduled(WorkOrder.VehicleId, request.NewStartDateTimeUtc, newEndAt, WorkOrder.Id, cancellationToken))
        {
            logger.LogWarning("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", WorkOrder.VehicleId);
            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var IsSpotAvailable = await policy.CheckSpotAvailabilityAsync(request.NewStartDateTimeUtc, newEndAt, request.NewSpot, request.WorkOrderId, cancellationToken);
        if (IsSpotAvailable.IsError)
        {
            logger.LogWarning("Relocate Work Order Failed: Spot '{Spot}' is already occupied during this time range.", request.NewSpot.ToString());
            return ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot;
        }

        if (await policy.IsLaborOccupiedDuringRange(request.NewStartDateTimeUtc, newEndAt, WorkOrder.LaborId, WorkOrder.Id))
        {
            logger.LogWarning("Relocate Work Order Failed: Labor with Id '{LaborId}' is already occupied or unavailable during this time range.", WorkOrder.LaborId);
            return ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime;
        }

        var UpdateStartTimeState = WorkOrder.ReLocateWorkOrder(request.NewSpot, request.NewStartDateTimeUtc, newEndAt);
        if (UpdateStartTimeState.IsError)
        {
            logger.LogWarning("Relocate Work Order Failed: Domain validation failed. Error: {Error}", UpdateStartTimeState.TopError.Description);
            return UpdateStartTimeState.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        return Result.Updated;
    }
}
