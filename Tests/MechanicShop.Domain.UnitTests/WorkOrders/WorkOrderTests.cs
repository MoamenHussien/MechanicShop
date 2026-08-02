using MechanicShop.Tests.Common.RepaireTasks;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;

public class WorkOrderTests
{
    [Fact]
    public void Total_ShouldReturnCorrectValue()
    {
        // Arrange
        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            laborCost: 100m,
            parts:
            [
                PartFactory.CreatePart(cost: 50m, quantity: 2).Value
            ]).Value;

        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            laborCost: 200m,
            parts:
            [
                PartFactory.CreatePart(cost: 20m, quantity: 5).Value
            ]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: [repairTask1, repairTask2]).Value;

        // Assert
        Assert.Equal(500m, workOrder.Total);
    }

    [Fact]
    public void TotalPartsCost_ShouldReturnCorrectValue()
    {
        // Arrange
        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            parts:
            [
                PartFactory.CreatePart(cost: 50m, quantity: 2).Value
            ]).Value;

        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            parts:
            [
                PartFactory.CreatePart(cost: 20m, quantity: 5).Value
            ]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: [repairTask1, repairTask2]).Value;

        // Assert
        Assert.Equal(200m, workOrder.TotalPartsCost);
    }

    [Fact]
    public void TotalLaborCost_ShouldReturnCorrectValue()
    {
        // Arrange
        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            laborCost: 100m).Value;

        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            laborCost: 200m).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: [repairTask1, repairTask2]).Value;

        // Assert
        Assert.Equal(300m, workOrder.TotalLaborCost);
    }

    [Fact]
    public void IsEditable_ShouldReturnTrue_WhenStateIsScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Assert
        Assert.True(workOrder.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldReturnFalse_WhenStateIsInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Assert
        Assert.False(workOrder.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldReturnFalse_WhenStateIsCompleted()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();

        // Assert
        Assert.False(workOrder.IsEditable);
    }

    [Fact]
    public void IsEditable_ShouldReturnFalse_WhenStateIsCancelled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsCancelled();

        // Assert
        Assert.False(workOrder.IsEditable);
    }

    [Fact]
    public void IsDeletable_ShouldReturnTrue_WhenStateIsScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Assert
        Assert.True(workOrder.IsDeletable);
    }

    [Fact]
    public void IsDeletable_ShouldReturnTrue_WhenStateIsCancelled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsCancelled();

        // Assert
        Assert.True(workOrder.IsDeletable);
    }

    [Fact]
    public void IsDeletable_ShouldReturnFalse_WhenStateIsInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Assert
        Assert.False(workOrder.IsDeletable);
    }

    [Fact]
    public void IsDeletable_ShouldReturnFalse_WhenStateIsCompleted()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();

        // Assert
        Assert.False(workOrder.IsDeletable);
    }

    [Fact]
    public void CreateWorkOrder_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var laborId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);

        List<RepairTask> repairTasks =
        [
            RepairTaskFactory.CreateRepairTask().Value
        ];

        // Act
        var result = WorkOrderFactory.CreateWorkOrder(
            id: id,
            laborId: laborId,
            vehicleId: vehicleId,
            spot: Spot.A,
            startAt: startAt,
            endAt: endAt,
            repairTasks: repairTasks);

        // Assert
        Assert.True(result.IsSuccess);

        var workOrder = result.Value;

        Assert.Equal(id, workOrder.Id);
        Assert.Equal(laborId, workOrder.LaborId);
        Assert.Equal(vehicleId, workOrder.VehicleId);
        Assert.Equal(Spot.A, workOrder.Spot);
        Assert.Equal(startAt, workOrder.StartAtUtc);
        Assert.Equal(endAt, workOrder.EndAtUtc);
        Assert.Equal(WorkOrderState.Scheduled, workOrder.State);

        Assert.Single(workOrder.RepairTasks);
    }

    [Fact]
    public void CreateWorkOrder_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = WorkOrder.Create(
            id: Guid.Empty,
            LaborId: Guid.NewGuid(),
            VehicleId: Guid.NewGuid(),
            spot: Spot.A,
            StartAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidVehicleId()
    {
        // Act
        var result = WorkOrder.Create(
            id: Guid.NewGuid(),
            LaborId: Guid.NewGuid(),
            VehicleId: Guid.Empty,
            spot: Spot.A,
            StartAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.VehicleIdRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidLaborId()
    {
        // Act
        var result = WorkOrder.Create(
            id: Guid.NewGuid(),
            LaborId: Guid.Empty,
            VehicleId: Guid.NewGuid(),
            spot: Spot.A,
            StartAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.LaborIdRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidSpot()
    {
        // Act
        var result = WorkOrderFactory.CreateWorkOrder(
            spot: (Spot)999);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.SpotInvalid.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidStartTime()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddMinutes(-10);
        var end = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var result = WorkOrderFactory.CreateWorkOrder(
            startAt: start,
            endAt: end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.InvalidStartingTiming.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidEndTime()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start;

        // Act
        var result = WorkOrderFactory.CreateWorkOrder(
            startAt: start,
            endAt: end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.InvalidEndingTiming.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithInvalidRepairTasks()
    {
        // Act
        var result = WorkOrder.Create(
            id: Guid.NewGuid(),
            LaborId: Guid.NewGuid(),
            VehicleId: Guid.NewGuid(),
            spot: Spot.A,
            StartAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndAt: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            repairTasks: null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.RepairTasksRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WithEmptyRepairTasks()
    {
        // Act
        var result = WorkOrderFactory.CreateWorkOrder(
            repairTasks: []);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.RepairTasksRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void ReAssignLabor_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var newLaborId = Guid.NewGuid();

        // Act
        var result = workOrder.ReAssignLabor(newLaborId);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(newLaborId, workOrder.LaborId);
    }

    [Fact]
    public void ReAssignLabor_ShouldFail_WhenWorkOrderIsNotScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.ReAssignLabor(Guid.NewGuid());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantEditWorkOrder(WorkOrderState.InProgress).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSucceed_WhenStateIsScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Act
        var result = workOrder.MarkAsDeleted();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSucceed_WhenStateIsCancelled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsCancelled();

        // Act
        var result = workOrder.MarkAsDeleted();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public void MarkAsDeleted_ShouldFail_WhenStateIsInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.MarkAsDeleted();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantDeleteWorkOrder(WorkOrderState.InProgress).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsDeleted_ShouldFail_WhenStateIsCompleted()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();

        // Act
        var result = workOrder.MarkAsDeleted();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantDeleteWorkOrder(WorkOrderState.Completed).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsCompleted_ShouldSucceed_WhenStateIsInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.MarkAsCompleted();

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(WorkOrderState.Completed, workOrder.State);
    }

    [Fact]
    public void MarkAsCompleted_ShouldFail_WhenStateIsNotInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Act
        var result = workOrder.MarkAsCompleted();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantMarkAsComplete(WorkOrderState.Scheduled).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsInProgress_ShouldSucceed_WhenStateIsScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Act
        var result = workOrder.MarkAsInProgress();

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(WorkOrderState.InProgress, workOrder.State);
    }

    [Fact]
    public void MarkAsInProgress_ShouldFail_WhenStateIsNotScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.MarkAsInProgress();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantMarkAsInProgress(WorkOrderState.InProgress).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsCancelled_ShouldSucceed_WhenStateIsScheduled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Act
        var result = workOrder.MarkAsCancelled();

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
    }

    [Fact]
    public void MarkAsCancelled_ShouldSucceed_WhenStateIsInProgress()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.MarkAsCancelled();

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(WorkOrderState.Cancelled, workOrder.State);
    }

    [Fact]
    public void MarkAsCancelled_ShouldFail_WhenStateIsCompleted()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();

        // Act
        var result = workOrder.MarkAsCancelled();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantMarkAsCancelled(WorkOrderState.Completed).Code,
            result.TopError.Code);
    }

    [Fact]
    public void MarkAsCancelled_ShouldFail_WhenStateIsAlreadyCancelled()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsCancelled();

        // Act
        var result = workOrder.MarkAsCancelled();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.CantMarkAsCancelled(WorkOrderState.Cancelled).Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var start = workOrder.StartAtUtc.AddHours(2);
        var end = start.AddHours(2);

        // Act
        var result = workOrder.UpdateTiming(start, end);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(start, workOrder.StartAtUtc);
        Assert.Equal(end, workOrder.EndAtUtc);
    }

    [Fact]
    public void UpdateTiming_ShouldFail_WithInvalidTiming()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var start = workOrder.StartAtUtc.AddHours(2);
        var end = start;

        // Act
        var result = workOrder.UpdateTiming(start, end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.InvalidEndingTiming.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldFail_WhenWorkOrderIsNotEditable()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();
        workOrder.MarkAsCompleted();

        var start = DateTimeOffset.UtcNow.AddHours(5);
        var end = start.AddHours(2);

        // Act
        var result = workOrder.UpdateTiming(start, end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.TimingReadonly(
                workOrder.Id.ToString(),
                WorkOrderState.Completed).Code,
            result.TopError.Code);
    }

    [Fact]
    public void ReLocateWorkOrder_ShouldSucceed_WithValidData()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var start = workOrder.StartAtUtc.AddHours(1);
        var end = start.AddHours(2);

        // Act
        var result = workOrder.ReLocateWorkOrder(
            Spot.B,
            start,
            end);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Spot.B, workOrder.Spot);
        Assert.Equal(start, workOrder.StartAtUtc);
        Assert.Equal(end, workOrder.EndAtUtc);
    }

    [Fact]
    public void ReLocateWorkOrder_ShouldFail_WithInvalidSpot()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        // Act
        var result = workOrder.ReLocateWorkOrder(
            (Spot)999,
            workOrder.StartAtUtc,
            workOrder.EndAtUtc);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.SpotInvalid.Code,
            result.TopError.Code);
    }

    [Fact]
    public void ReLocateWorkOrder_ShouldFail_WithInvalidTiming()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var start = workOrder.StartAtUtc.AddHours(2);
        var end = start;

        // Act
        var result = workOrder.ReLocateWorkOrder(
            Spot.B,
            start,
            end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.InvalidEndingTiming.Code,
            result.TopError.Code);
    }

    [Fact]
    public void RemoveAndInsertRepairTasks_ShouldSucceed_WithValidData()
    {
        // Arrange
        var oldTask = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: [oldTask]).Value;

        var newTask = RepairTaskFactory.CreateRepairTask(name: "Brake").Value;

        // Act
        var result = workOrder.RemoveAndInsertRepairTasks(
            [oldTask, newTask]);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, workOrder.RepairTasks.Count);

        Assert.Contains(
            workOrder.RepairTasks,
            n => n.Id == oldTask.Id);

        Assert.Contains(
            workOrder.RepairTasks,
            n => n.Id == newTask.Id);
    }

    [Fact]
    public void RemoveAndInsertRepairTasks_ShouldRemoveRepairTasksNotIncluded()
    {
        // Arrange
        var task1 = RepairTaskFactory.CreateRepairTask().Value;
        var task2 = RepairTaskFactory.CreateRepairTask().Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            repairTasks: [task1, task2]).Value;

        // Act
        var result = workOrder.RemoveAndInsertRepairTasks([task2]);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Single(workOrder.RepairTasks);
        Assert.Equal(task2.Id, workOrder.RepairTasks.Single().Id);
    }

    [Fact]
    public void RemoveAndInsertRepairTasks_ShouldFail_WhenWorkOrderIsNotEditable()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.MarkAsInProgress();

        // Act
        var result = workOrder.RemoveAndInsertRepairTasks(
            [RepairTaskFactory.CreateRepairTask().Value]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            WorkOrderErrors.Readonly.Code,
            result.TopError.Code);
    }
}
