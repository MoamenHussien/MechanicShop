using System.Reflection;
using MechanicShop.Tests.Common.Security;

namespace MechanicShop.Api.IntegrationTests.Common;

public interface ITestDataBuilder<T>
{
    T Build();
}

public class WorkOrderTestDataBuilder : ITestDataBuilder<WorkOrder>
{
    private Guid _id = Guid.NewGuid();
    private Guid _vehicleId = Guid.NewGuid();
    private DateTimeOffset _startAt = DateTimeOffset.UtcNow.AddHours(1);
    private DateTimeOffset _endAt = DateTimeOffset.UtcNow.AddHours(3);
    private Guid _laborId = TestUsers.Labor01.Id;
    private Spot _spot = Spot.A;
    private List<RepairTask> _repairTasks = [];
    private WorkOrderState _state = WorkOrderState.Scheduled;

    public static WorkOrderTestDataBuilder Create() => new();

    public WorkOrderTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public WorkOrderTestDataBuilder WithVehicle(Guid vehicleId)
    {
        _vehicleId = vehicleId;
        return this;
    }

    public WorkOrderTestDataBuilder WithTimeSlot(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        _startAt = startAt;
        _endAt = endAt;
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(string laborId)
    {
        _laborId = Guid.Parse(laborId);
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(Guid laborId)
    {
        _laborId = laborId;
        return this;
    }

    public WorkOrderTestDataBuilder AtSpot(Spot spot)
    {
        _spot = spot;
        return this;
    }

    public WorkOrderTestDataBuilder WithRepairTasks(params RepairTask[] repairTasks)
    {
        _repairTasks = [.. repairTasks];
        return this;
    }

    public WorkOrderTestDataBuilder WithRepairTasks(List<RepairTask> repairTasks)
    {
        _repairTasks = repairTasks;
        return this;
    }

    public WorkOrderTestDataBuilder WithState(WorkOrderState state)
    {
        _state = state;
        return this;
    }

    public WorkOrderTestDataBuilder ForToday(TimeOnly? from = null, TimeOnly? to = null)
    {
        if (from.HasValue && to.HasValue)
        {
            var today = DateTimeOffset.UtcNow.Date;
            _startAt = today.Add(from.Value.ToTimeSpan());
            _endAt = today.Add(to.Value.ToTimeSpan());
        }
        else
        {
            // Default to future times (1-3 hours from now) to pass WorkOrder.Create validation
            _startAt = DateTimeOffset.UtcNow.AddHours(1);
            _endAt = DateTimeOffset.UtcNow.AddHours(3);
        }

        return this;
    }

    public WorkOrderTestDataBuilder InProgress()
    {
        _state = WorkOrderState.InProgress;
        _startAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        return this;
    }

    public WorkOrderTestDataBuilder Completed()
    {
        _state = WorkOrderState.Completed;
        _startAt = DateTimeOffset.UtcNow.AddHours(-3);
        _endAt = DateTimeOffset.UtcNow.AddHours(-1);
        return this;
    }

    public WorkOrder Build()
    {
        // Always create with safe future times to pass WorkOrder.Create domain validation
        var safeStartAt = DateTimeOffset.UtcNow.AddHours(1);
        var safeEndAt = DateTimeOffset.UtcNow.AddHours(3);

        var workOrder = WorkOrder.Create(
            id: _id,
            VehicleId: _vehicleId,
            StartAtUtc: safeStartAt,
            EndAt: safeEndAt,
            LaborId: _laborId,
            spot: _spot,
            repairTasks: _repairTasks).Value;

        // Create with future times to satisfy domain creation validation,
        // then override the timestamps to simulate an existing work order
        // for integration tests
        SetPrivateProperty(workOrder, nameof(WorkOrder.StartAtUtc), _startAt);
        SetPrivateProperty(workOrder, nameof(WorkOrder.EndAtUtc), _endAt);

        // Set state if different from default
        if (_state != WorkOrderState.Scheduled)
        {
            SetPrivateProperty(workOrder, nameof(WorkOrder.State), _state);
        }

        return workOrder;
    }

    private static void SetPrivateProperty<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property!.SetValue(target, value);
    }
}
