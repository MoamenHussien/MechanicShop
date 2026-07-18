using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Security.AccessControl;
using FluentValidation;
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
        RuleFor(n => n.repairTasksIds).NotEmpty().WithMessage("The Repair Task Is Required").Must(n => n.Count> 0).WithMessage("At Least One Repair Task Required");
        RuleFor(n => n.spot).IsInEnum().WithErrorCode("Spot_Invalid").WithMessage("Spot must be a valid Spot value. [A, B, C, D]");
        RuleFor(n => n.StartAtUtc).GreaterThan(DateTimeOffset.UtcNow).WithMessage("Start time must be in the future");
        RuleFor(n => n.repairTasksIds).NotEmpty().WithMessage("At Least One Repair Task Required");
        RuleForEach(n => n.repairTasksIds).Must(n => n != Guid.Empty).WithMessage("Each RepairTaskId must be a valid non-empty GUID");

    }
}

public class CreateWorkOrderCommandHandler(ILogger<CreateWorkOrderCommandHandler> logger, IAppDbContext context, HybridCache cache, IWorkOrderPolicy policy)
: IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // if(await identity.IsInRoleAsync(user.Id!.Value,Role.Labor.ToString()))
        // {
        //     logger.LogWarning("Create Work Order : (Forbidden) , This User {UserId} is not allowed to Create New Work Order", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }

        var Selected_Repair_Tasks = await context.RepairTasks.Where(n => request.repairTasksIds.Contains(n.Id)).Include(n=>n.Parts).ToListAsync(cancellationToken);
        if (!Selected_Repair_Tasks.Any())
        {
            logger.LogError("Not Found Any Repair Tasks For These Ids : {ids}", string.Join(" , ", request.repairTasksIds));
            return ApplicationErrors.NotFoundAnyRepairTasks;
        }

        if (Selected_Repair_Tasks.Count != request.repairTasksIds.Count)
        {
            var notfound = request.repairTasksIds.Except(Selected_Repair_Tasks.Select(n => n.Id));
            logger.LogError("Cant Find Some Selected Repair Tasks , The Ids For Repair Tasks Not Found : {ids}", string.Join(" , ", notfound));
            return ApplicationErrors.SomeRepairTaskIdsNotfound;
        }

        var TotalWorkOrdersDurationInMins = TimeSpan.FromMinutes(Selected_Repair_Tasks.Sum(n => (int)n.EstimatedDuration));
        var EndAt = request.StartAtUtc.Add(TotalWorkOrdersDurationInMins);

        var IsOutsideOperatingHours = policy.IsOutsideOperatingHours(request.StartAtUtc, TotalWorkOrdersDurationInMins);
        if (IsOutsideOperatingHours)
        {
            logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", request.StartAtUtc, EndAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.StartAtUtc, EndAt);
        }

        var checkMinRequirementResult = policy.ValidateMinimumRequirement(request.StartAtUtc, EndAt);
        if (checkMinRequirementResult.IsError)
        {
            logger.LogError("WorkOrder duration is shorter than the configured minimum.");
            return checkMinRequirementResult.Errors;
        }

        var Is_this_range_time_free = await policy.CheckSpotAvailabilityAsync(request.StartAtUtc, EndAt, request.spot, null, cancellationToken);
        if (Is_this_range_time_free.IsError)
        {
            logger.LogError("Spot: {Spot} is not available.", request.spot.ToString());
            return ApplicationErrors.RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot;
        }

        var VehicleId_exists = await context.Vehicles.Where(n => n.Id == request.VehicleId).Include(n=>n.VehicleModel).ThenInclude(n=>n.VehicleMake).Include(n => n.Customer).FirstOrDefaultAsync(cancellationToken);
        if (VehicleId_exists is null)
        {
            logger.LogError("Vehicle with Id '{VehicleId}' does not exist.", request.VehicleId);
            return ApplicationErrors.NotFoundThisVehicleInfo;
        }

        var SelectedLabor = await context.Employees.FindAsync([request.LaborId], cancellationToken);
        if (SelectedLabor is null)
        {
            logger.LogError("Invalid LaborId: {LaborId}", request.LaborId.ToString());
            return ApplicationErrors.NotFoundTheLabor;
        }

        var IsLaborAvailable = await policy.IsLaborOccupiedDuringRange(request.StartAtUtc, EndAt, request.LaborId,null,cancellationToken);
        if (IsLaborAvailable)
        {
            logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", request.LaborId);
            return ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime;
        }

        var is_VehicleId_Has_Active_WorkOrder_Now = await policy.IsVehicleAlreadyScheduled(request.VehicleId, request.StartAtUtc, EndAt,null,cancellationToken);
        if (is_VehicleId_Has_Active_WorkOrder_Now)
        {
            logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", request.VehicleId);
            return ApplicationErrors.VehicleSchedulingConflict;
        }


        var CreateWorkOrderStatus = WorkOrder.Create(Guid.NewGuid(), request.LaborId, request.VehicleId, request.spot, request.StartAtUtc, EndAt, Selected_Repair_Tasks);

        if (CreateWorkOrderStatus.IsError)
        {
            logger.LogError("Failed to create WorkOrder: {Error}", CreateWorkOrderStatus.TopError.Description);
            return CreateWorkOrderStatus.Errors;
        }

        await context.WorkOrders.AddAsync(CreateWorkOrderStatus.Value, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("WorkOrders", cancellationToken);

        logger.LogInformation("Created WorkOrder with Id: {WorkOrderId}, saved changes to database, and removed cache tag 'WorkOrders'. VehicleId: {VehicleId}, LaborId: {LaborId}, StartAtUtc: {StartAt}, EndAtUtc: {EndAt}",CreateWorkOrderStatus.Value.Id,CreateWorkOrderStatus.Value.VehicleId,CreateWorkOrderStatus.Value.LaborId,CreateWorkOrderStatus.Value.StartAtUtc,CreateWorkOrderStatus.Value.EndAtUtc );

        CreateWorkOrderStatus.Value.Labor = SelectedLabor;
        CreateWorkOrderStatus.Value.Vehicle = VehicleId_exists;

        return CreateWorkOrderStatus.Value.ToDto();
    }









}
