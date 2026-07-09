public interface IWorkOrderPolicy
{
   Task<bool> IsThisCustomerHasAnyRequestForWorkOrderBeforeAsync(Guid CustomerId,CancellationToken ct = default);
   Task<bool> IsLaborOccupiedDuringRange(DateTimeOffset StartAtUtc,DateTimeOffset EndAtUtc ,Guid labor,Guid? excludeWorkOrderId=null,CancellationToken ct = default );
   Task<Result<Success>>CheckSpotAvailabilityAsync( DateTimeOffset startAt, DateTimeOffset endAt,
                                             Spot spot , Guid? excludeWorkOrderId = null, CancellationToken ct = default);
   bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration);
   Task<bool> IsVehicleAlreadyScheduled(Guid vehicleId, DateTimeOffset startAt, DateTimeOffset endAt, Guid? excludedWorkOrderId = null,CancellationToken ct = default);
   Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt);

}