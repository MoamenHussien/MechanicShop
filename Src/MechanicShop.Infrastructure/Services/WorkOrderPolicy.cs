using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class WorkOrderPolicy(AppDbContext context, IOptions<AppSettings> options) : IWorkOrderPolicy
{
    public async Task<Result<Success>> CheckSpotAvailabilityAsync(DateTimeOffset startAt, DateTimeOffset endAt, Spot spot, Guid? excludeWorkOrderId, CancellationToken ct)
    {
        var isOccupied = await context.WorkOrders.AnyAsync(n =>
                        n.Spot == spot &&
                        n.StartAtUtc < endAt &&
                        n.EndAtUtc > startAt &&
                        n.State != WorkOrderState.Cancelled &&
                        (!excludeWorkOrderId.HasValue || n.Id != excludeWorkOrderId.Value),
                        ct);

        return isOccupied
             ? Error.Conflict("MechanicShop_Spot_Full", "The selected time slot is unavailable for the requested services.")
             : Result.Success;
    }

    public async Task<bool> IsLaborOccupiedDuringRange(DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, Guid labor, Guid? excludeWorkOrderId = null, CancellationToken ct = default)
    {
        return await context.WorkOrders.AnyAsync(n => n.StartAtUtc < EndAtUtc && n.EndAtUtc > StartAtUtc && n.LaborId == labor && n.State != WorkOrderState.Cancelled && (!excludeWorkOrderId.HasValue || n.Id != excludeWorkOrderId), ct);
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
    {
        //Duration =2 hours                   // start at    9AM
        var opening = startAt.Date.Add(options.Value.OpeningTime.ToTimeSpan());  //    8AM
        var closing = startAt.Date.Add(options.Value.ClosingTime.ToTimeSpan()); //    12AM
        var endAt = startAt + duration;                               // 9 + 2         11Am     
        return startAt < opening || endAt > closing;
    }

    public async Task<bool> IsThisCustomerHasAnyRequestForWorkOrderBeforeAsync(Guid CustomerId, CancellationToken ct = default)
    {
        return await context.WorkOrders.AnyAsync(n => n.Vehicle.CustomerId == CustomerId && n.State != WorkOrderState.Cancelled,ct);
    }

    public async Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null, CancellationToken ct = default)
    {
        return await context.WorkOrders.AnyAsync(n => n.VehicleId == vehicleId && n.StartAtUtc < endAt && n.EndAtUtc > startAt && n.State != WorkOrderState.Cancelled && (!excludedWorkOrderId.HasValue || n.Id != excludedWorkOrderId ), ct);
    }

    public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if ((endAt - startAt) < TimeSpan.FromMinutes(options.Value.MinimumAppointmentDurationInMinutes))
        {
            return Error.Conflict(
                "WorkOrder_TooShort",
                $"WorkOrder duration must be at least {options.Value.MinimumAppointmentDurationInMinutes} minutes.");
        }

        return Result.Success;
    }
}