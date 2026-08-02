using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Security.AccessControl;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record CreateWorkOrderCommand(Guid LaborId, Guid VehicleId, Spot spot, DateTimeOffset StartAtUtc, List<Guid> repairTasksIds) : IRequest<Result<WorkOrderDto>>;

public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(n => n.LaborId).IdRequired("Labor");
        RuleFor(n => n.VehicleId).IdRequired("Vehicle");
        RuleFor(n => n.repairTasksIds).NotEmpty().WithMessage("The Repair Task Is Required").Must(n => n.Count > 0).WithMessage("At Least One Repair Task Required");
        RuleFor(n => n.spot).IsInEnum().WithErrorCode("Spot_Invalid").WithMessage("Spot must be a valid Spot value. [A, B, C, D]");
        RuleFor(n => n.StartAtUtc).GreaterThan(DateTimeOffset.UtcNow).WithMessage("Start time must be in the future");
        RuleFor(n => n.repairTasksIds).NotEmpty().WithMessage("At Least One Repair Task Required");
        RuleForEach(n => n.repairTasksIds).Must(n => n != Guid.Empty).WithMessage("Each RepairTaskId must be a valid non-empty GUID");
    }
}

public class CreateWorkOrderCommandHandler(ILogger<CreateWorkOrderCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator, IWorkOrderPolicy policy)
: IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // if(await identity.IsInRoleAsync(user.Id!.Value,Role.Labor.ToString()))
        // {
        //     logger.LogWarning("Create Work Order : (Forbidden) , This User {UserId} is not allowed to Create New Work Order", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }
        var selected_Repair_Tasks = await context.RepairTasks.Where(n => request.repairTasksIds.Contains(n.Id)).Include(n => n.Parts).ToListAsync(cancellationToken);
        if (!selected_Repair_Tasks.Any())
        {
            logger.LogError("Not Found Any Repair Tasks For These Ids : {ids}", string.Join(" , ", request.repairTasksIds));
            return ApplicationErrors.NotFoundAnyRepairTasks;
        }

        if (selected_Repair_Tasks.Count != request.repairTasksIds.Count)
        {
            var notfound = request.repairTasksIds.Except(selected_Repair_Tasks.Select(n => n.Id));
            logger.LogError("Cant Find Some Selected Repair Tasks , The Ids For Repair Tasks Not Found : {ids}", string.Join(" , ", notfound));
            return ApplicationErrors.SomeRepairTaskIdsNotfound;
        }

        var totalWorkOrdersDurationInMins = TimeSpan.FromMinutes(selected_Repair_Tasks.Sum(n => (int)n.EstimatedDuration));
        var endAt = request.StartAtUtc.Add(totalWorkOrdersDurationInMins);

        var isOutsideOperatingHours = policy.IsOutsideOperatingHours(request.StartAtUtc, totalWorkOrdersDurationInMins);
        if (isOutsideOperatingHours)
        {
            logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", request.StartAtUtc, endAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.StartAtUtc, endAt);
        }

        var checkMinRequirementResult = policy.ValidateMinimumRequirement(request.StartAtUtc, endAt);
        if (checkMinRequirementResult.IsError)
        {
            logger.LogError("WorkOrder duration is shorter than the configured minimum.");
            return checkMinRequirementResult.Errors;
        }

        var is_this_range_time_free = await policy.CheckSpotAvailabilityAsync(request.StartAtUtc, endAt, request.spot, null, cancellationToken);
        if (is_this_range_time_free.IsError)
        {
            logger.LogError("Spot: {Spot} is not available.", request.spot.ToString());
            return ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot;
        }

        var vehicleId_exists = await context.Vehicles.Where(n => n.Id == request.VehicleId).Include(n => n.VehicleModel).ThenInclude(n => n.VehicleMake).Include(n => n.Customer).FirstOrDefaultAsync(cancellationToken);
        if (vehicleId_exists is null)
        {
            logger.LogError("Vehicle with Id '{VehicleId}' does not exist.", request.VehicleId);
            return ApplicationErrors.NotFoundThisVehicleInfo;
        }

        var selectedLabor = await context.Employees.FindAsync([request.LaborId], cancellationToken);
        if (selectedLabor is null)
        {
            logger.LogError("Invalid LaborId: {LaborId}", request.LaborId.ToString());
            return ApplicationErrors.NotFoundTheLabor;
        }

        var isLaborAvailable = await policy.IsLaborOccupiedDuringRange(request.StartAtUtc, endAt, request.LaborId, null, cancellationToken);
        if (isLaborAvailable)
        {
            logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", request.LaborId);
            return ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime;
        }

        var is_VehicleId_Has_Active_WorkOrder_Now = await policy.IsVehicleAlreadyScheduled(request.VehicleId, request.StartAtUtc, endAt, null, cancellationToken);
        if (is_VehicleId_Has_Active_WorkOrder_Now)
        {
            logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", request.VehicleId);
            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var createWorkOrderStatus = WorkOrder.Create(Guid.NewGuid(), request.LaborId, request.VehicleId, request.spot, request.StartAtUtc, endAt, selected_Repair_Tasks);

        if (createWorkOrderStatus.IsError)
        {
            logger.LogError("Failed to create WorkOrder: {Error}", createWorkOrderStatus.TopError.Description);
            return createWorkOrderStatus.Errors;
        }

        await context.WorkOrders.AddAsync(createWorkOrderStatus.Value, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        logger.LogInformation("Created WorkOrder with Id: {WorkOrderId}, saved changes to database, and removed cache tag 'WorkOrders'. VehicleId: {VehicleId}, LaborId: {LaborId}, StartAtUtc: {StartAt}, EndAtUtc: {EndAt}", createWorkOrderStatus.Value.Id, createWorkOrderStatus.Value.VehicleId, createWorkOrderStatus.Value.LaborId, createWorkOrderStatus.Value.StartAtUtc, createWorkOrderStatus.Value.EndAtUtc);

        createWorkOrderStatus.Value.Labor = selectedLabor;
        createWorkOrderStatus.Value.Vehicle = vehicleId_exists;

        return createWorkOrderStatus.Value.ToDto();
    }
}
