public static class WorkOrderErrors
{
    public static readonly Error VehicleIdRequired = Error.Validation(
        code: "WorkOrderErrors.VehicleIdRequired",
        description: "Vehicle Id is required");

    public static readonly Error RepairTasksRequired = Error.Validation(
        code: "WorkOrderErrors.RepairTasksRequired",
        description: "At least one repair task is required");

    public static readonly Error LaborIdRequired = Error.Validation(
        code: "WorkOrderErrors.LaborIdRequired",
        description: "Labor Id is required");

    public static readonly Error InvalidEndingTiming = Error.Conflict(
        code: "WorkOrderErrors.InvalidTiming",
        description: "End time must be after start time.");

    public static readonly Error InvalidStartingTiming = Error.Conflict(
    code: "WorkOrderErrors.InvalidStartingTiming",
    description: "Start time must be in the future");

    public static readonly Error SpotInvalid = Error.Validation(
        code: "WorkOrderErrors.SpotInvalid",
        description: "The provided spot is invalid");

    public static readonly Error Readonly = Error.Conflict(
        code: "WorkOrderErrors.Readonly",
        description: "WorkOrder is read-only.");

    public static Error TimingReadonly(string id, WorkOrderState state) => Error.Conflict(
        code: "WorkOrderErrors.TimingReadonly",
        description: $"WorkOrder '{id}': Can't Modify timing when WorkOrder status is '{state}'.");

    public static Error LaborIdEmpty(string id) => Error.Validation(
        code: "WorkOrderErrors.LaborIdEmpty",
        description: $"WorkOrder '{id}': Labor Id is empty");

    public static Error StateTransitionNotAllowed(DateTimeOffset startAtUtc) => Error.Conflict(
       code: "WorkOrderErrors.StateTransitionNotAllowed",
       description: $"State transition is not allowed before the work order’s scheduled start time {startAtUtc:yyyy-MM-dd HH:mm} UTC.");

    public static Error InvalidStateTransition(WorkOrderState current, WorkOrderState next) => Error.Conflict(
        code: "WorkOrderErrors.InvalidStateTransition",
        description: $"WorkOrder Invalid State transition from '{current}' to '{next}'.");

    public static readonly Error RepairTaskAlreadyAdded = Error.Conflict(
        code: "WorkOrderErrors.RepairTaskAlreadyAdded",
        description: "Repair task already exists.");

    public static readonly Error InvalidStateTransitionTime = Error.Conflict(
        code: "WorkOrderErrors.InvalidStateTransitionTime",
        description: "State transition is not allowed before the work order’s scheduled start time.");

    public static Error CantMarkAsComplete(WorkOrderState state) =>
        Error.Conflict(
            "Invalid WorkOrder State",
            $"Cannot mark as Completed from '{state}'.");

    public static Error CantMarkAsInProgress(WorkOrderState state) =>
        Error.Conflict(
            "Invalid WorkOrder State",
            $"Cannot mark as InProgress from '{state}'.");

    public static Error CantMarkAsCancelled(WorkOrderState state) =>
        Error.Conflict(
            "Invalid WorkOrder State",
            $"Cannot mark as Cancelled from '{state}'.");

    public static Error CantDeleteWorkOrder(WorkOrderState state) => Error.Conflict($"WorkOrder Status: {state}", $"Cannot delete this work order because its current status is '{state}', which does not allow deletion.");

    public static Error CantEditWorkOrder(WorkOrderState state) => Error.Conflict($"WorkOrder Status: {state}", $"Cannot edit this work order because its current status is '{state}', which does not allow editing.");
}
