using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

public class WorkOrder : AuditableEntity
{
    private readonly List<RepairTask> _RepairTasks = [];

    public IReadOnlyList<RepairTask> RepairTasks => _RepairTasks;

    public Invoice? Invoice { get; set; }

    public Employee Labor { get; set; } = null!;

    public Guid LaborId { get; private set; }

    public Vehicle Vehicle { get; set; } = null!;

    public Guid VehicleId { get; init; }

    public WorkOrderState State { get; private set; }

    public decimal? Discount { get; private set; }

    public decimal? Tax { get; private set; }

    public decimal Total => _RepairTasks.Sum(n => n.TotalCost);

    // public decimal Total => _RepairTasks.Sum(n => n.TotalCost) + (Tax ?? 0) - (Discount ?? 0);
    public decimal TotalPartsCost => _RepairTasks.Sum(n => n.TotalPartsCost);

    public decimal TotalLaborCost => _RepairTasks.Sum(n => n.LaborCost);

    public bool IsEditable => State is not (WorkOrderState.Cancelled or WorkOrderState.Completed or WorkOrderState.InProgress);

    public bool IsDeletable => State is (WorkOrderState.Scheduled or WorkOrderState.Cancelled);

    public Spot Spot { get; private set; }

    public DateTimeOffset EndAtUtc { get; private set; }

    public DateTimeOffset StartAtUtc { get; private set; }

#pragma warning disable CS8618
    private WorkOrder()
    {
    }
#pragma warning restore CS8618

    private WorkOrder(Guid id, Guid LaborId, Guid VehicleId, Spot spot, DateTimeOffset StartAtUtc, DateTimeOffset EndAt, WorkOrderState status, List<RepairTask> repairTasks)
        : base(id)
    {
        this.LaborId = LaborId;
        this.Spot = spot;
        this.StartAtUtc = StartAtUtc;
        this.VehicleId = VehicleId;
        this._RepairTasks = repairTasks;
        this.State = status;
        this.EndAtUtc = EndAt;
    }

    public static Result<WorkOrder> Create(Guid id, Guid LaborId, Guid VehicleId, Spot spot, DateTimeOffset StartAtUtc, DateTimeOffset EndAt, List<RepairTask> repairTasks)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (VehicleId == Guid.Empty)
        {
            return WorkOrderErrors.VehicleIdRequired;
        }

        if (LaborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        if (!Enum.IsDefined(typeof(Spot), spot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        if (StartAtUtc < DateTimeOffset.UtcNow)
        {
            return WorkOrderErrors.InvalidStartingTiming;
        }

        if (EndAt <= StartAtUtc)
        {
            return WorkOrderErrors.InvalidEndingTiming;
        }

        if (repairTasks is null || repairTasks.Count == 0)
        {
            return WorkOrderErrors.RepairTasksRequired;
        }

        var workOrder = new WorkOrder(id, LaborId, VehicleId, spot, StartAtUtc, EndAt, WorkOrderState.Scheduled, repairTasks);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        return workOrder;
    }

    public Result<Updated> ReAssignLabor(Guid LaborID)
    {
        if (this.State is WorkOrderState.Scheduled)
        {
            this.LaborId = LaborID;
            this.AddDomainEvent(new WorkOrderCollectionModified());
            return Result.Updated;
        }
        else
        {
            return WorkOrderErrors.CantEditWorkOrder(this.State);
        }
    }

    public Result<Deleted> MarkAsDeleted()
    {
        if (!this.IsDeletable)
        {
            return WorkOrderErrors.CantDeleteWorkOrder(this.State);
        }

        this.AddDomainEvent(new WorkOrderCollectionModified());
        return Result.Deleted;
    }

    public Result<Updated> MarkAsCompleted()
    {
        if (this.State is WorkOrderState.InProgress)
        {
            this.State = WorkOrderState.Completed;
            this.AddDomainEvent(new WorkOrderCollectionModified());
            this.AddDomainEvent(new WorkOrderCompleted { WorkOrderId = this.Id });
            return Result.Updated;
        }
        else
        {
            return WorkOrderErrors.CantMarkAsComplete(this.State);
        }
    }

    public Result<Updated> MarkAsInProgress()
    {
        if (this.State is WorkOrderState.Scheduled)
        {
            this.State = WorkOrderState.InProgress;
            this.AddDomainEvent(new WorkOrderCollectionModified());
            return Result.Updated;
        }
        else
        {
            return WorkOrderErrors.CantMarkAsInProgress(this.State);
        }
    }

    public Result<Updated> MarkAsCancelled()
    {
        if (this.State is not (WorkOrderState.Completed or WorkOrderState.Cancelled))
        {
            this.State = WorkOrderState.Cancelled;
            this.AddDomainEvent(new WorkOrderCollectionModified());
            return Result.Updated;
        }
        else
        {
            return WorkOrderErrors.CantMarkAsCancelled(this.State);
        }
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.TimingReadonly(Id.ToString(), State);
        }

        if (endAt <= startAt)
        {
            return WorkOrderErrors.InvalidEndingTiming;
        }

        StartAtUtc = startAt;
        EndAtUtc = endAt;

        this.AddDomainEvent(new WorkOrderCollectionModified());

        return Result.Updated;
    }

    public Result<Updated> ReLocateWorkOrder(Spot NewSpot, DateTimeOffset NewStartDatetimeUtc, DateTimeOffset NewEndDateTimeUtc)
    {
        if (!Enum.IsDefined(typeof(Spot), NewSpot))
        {
            return WorkOrderErrors.SpotInvalid;
        }

        this.Spot = NewSpot;

        var updateTimeState = this.UpdateTiming(NewStartDatetimeUtc, NewEndDateTimeUtc);
        if (updateTimeState.IsError)
        {
            return updateTimeState.Errors;
        }

        return Result.Updated;
    }

    public Result<Updated> RemoveAndInsertRepairTasks(List<RepairTask> repairTasks)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.Readonly;
        }

        var newIds = repairTasks.Select(x => x.Id).ToHashSet();

        _RepairTasks.RemoveAll(x => !newIds.Contains(x.Id));

        var existingIds = _RepairTasks.Select(x => x.Id).ToHashSet();

        foreach (var repairTask in repairTasks)
        {
            if (!existingIds.Contains(repairTask.Id))
            {
                _RepairTasks.Add(repairTask);
            }
        }

        return Result.Updated;
    }
}
