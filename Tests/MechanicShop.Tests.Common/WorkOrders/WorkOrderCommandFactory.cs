

namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderCommandFactory
{
    public static CreateWorkOrderCommand CreateCreateWorkOrderCommand(
        Spot? spot = null,
        Guid? vehicleId = null,
        DateTimeOffset? startAt = null,
        List<Guid>? repairTaskIds = null,
        Guid? laborId = null)
    {
        return new CreateWorkOrderCommand(
             laborId ?? Guid.NewGuid(),
             vehicleId ?? Guid.NewGuid(),
             spot ?? Spot.A,
             startAt ?? DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(9),
             repairTaskIds ?? [Guid.NewGuid()]
           );
    }
}
