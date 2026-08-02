using MechanicShop.Tests.Common.RepaireTasks;

namespace MechanicShop.Tests.Common.WorkOrders;

public static class WorkOrderFactory
{
    public static Result<WorkOrder> CreateWorkOrder(
        Guid? id = null,
        Guid? vehicleId = null,
        DateTimeOffset? startAt = null,
        DateTimeOffset? endAt = null,
        Guid? laborId = null,
        Spot? spot = null,
        List<RepairTask>? repairTasks = null)
    {
        return WorkOrder.Create(
            id ?? Guid.NewGuid(),
            laborId ?? Guid.NewGuid(),
            vehicleId ?? Guid.NewGuid(),
            spot ?? Spot.A,
            startAt ?? DateTimeOffset.UtcNow.AddDays(1),
            endAt ?? DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            repairTasks ?? [RepairTaskFactory.CreateRepairTask().Value]);
    }
}
