using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;

public static class ApplicationErrors
{
    public static readonly Error MakeNotFound = Error.NotFound("The VehicleMake Is Not Found", "Make Not Found");
    public static readonly Error NotFoundAnyMakes = Error.NotFound("The VehiclesMakes Is Not Found", "Makes Not Found");
    public static readonly Error NotFoundAnyModelsToThisMakeId = Error.NotFound("The VehiclesModels Not Found To This Id", "Models Not Found");
    public static readonly Error TheCustomerNotFound = Error.NotFound("The Customer Not Found", "Customer Not Found");
    public static readonly Error NotFoundAnyCustomers = Error.NotFound("The Customers Not Found", "Not Found Any Customers");
    public static readonly Error TheCustomerHasRecordForWorkOrderBefore = Error.Conflict("Cannot delete customer: There are vehicles with maintenance records", "Has Record For Work Order Before");
    public static readonly Error CustomerWithThisEmailIsAlreadyExists = Error.Conflict("Customer_Email_Exists", "A customer with this email already exists");
    public static readonly Error InvalidAccessToken = Error.Unauthorized("Auth.ExpiredAccessToken.Invalid", "Expired access token is not valid.");
    public static readonly Error UserIdClaimInvalid = Error.Conflict(code: "Auth.UserIdClaim.Invalid", "Invalid userId claim.");
    public static readonly Error RefreshTokenExpiredOrInvalid = Error.Unauthorized("Auth.RefreshToken.Expired", "Refresh token is invalid or has expired.");
    public static readonly Error NotFoundAnyLabors = Error.NotFound("Not Found Any Of Labors", "Labors Is Empty");
    public static readonly Error NotFoundTheLabor = Error.NotFound("Not Found The Labor", "Labor Is Not Found");
    public static readonly Error NotFoundThisRepairTaskId = Error.NotFound("Not Found Any Repair Task With This Id", "The Repair Task Not Found");
    public static readonly Error NotFoundAnyRepairTasks = Error.NotFound("Not Found Any Repair Tasks", "The Repair Tasks Not Found");
    public static readonly Error SomeRepairTaskIdsNotfound = Error.NotFound("Some RepairTaskIds not found", "Not found Some RepairTaskIds");
    public static readonly Error NotFoundThisVehicleInfo = Error.NotFound("Not Found This Vehicle Info", "This Vehicle Not Found");
    public static readonly Error NotFoundTheVehicleModel = Error.NotFound("The Vehicle Model Not Found", "Vehicle Model Not Found");
    public static readonly Error RangeTimeIsAlreadyTakenByAnotherWorkOrderAtThisSpot = Error.Conflict("This time range is already booked by another Work Order At This Spot", "Time range conflict");
    public static readonly Error VehicleSchedulingConflict = Error.Conflict("Vehicle_Overlapping_WorkOrder", "The vehicle already has an overlapping WorkOrder.");
    public static readonly Error ThisLaborHasAnotherWorkOrderAtThisRangeTime = Error.Conflict("The labor is already occupied during the requested time", "Labor not available at this time range");
    public static Error WorkOrderOutsideOperatingHour(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) => Error.Conflict("ApplicationErrors.WorkOrder.Outside.OperatingHours", $"The WorkOrder time ({startAtUtc} ? {endAtUtc}) is outside of store operating hours.");
    public static readonly Error NotFoundTheWorkOrder = Error.NotFound("This Work Order Is Not Found", "Not Found This Work Order");
    public static Error CantDeleteWorkOrder(WorkOrderState state) => Error.Conflict($"WorkOrder Status: {state}", $"Cannot delete this work order because its current status is '{state}', which does not allow deletion.");
    public static Error CantEditWorkOrder(WorkOrderState state) => Error.Conflict($"WorkOrder Status: {state}", $"Cannot edit this work order because its current status is '{state}', which does not allow editing.");
    public static readonly Error NotAllowed = Error.Unauthorized("Identity.Forbidden", "You are not authorized to perform this action . This operation is restricted to administrative roles only");
    public static Error WorkOrderStartTimeNotComing(DateTimeOffset startWorkOrderTime) => Error.Validation("WorkOrder.StartTimeNotComing", $"State transition is not allowed before the work order's scheduled start time: '{startWorkOrderTime:g}'.");
    public static readonly Error NotAllowedToProcessWorkOrder = Error.Forbidden("NotAssignedWorkOrder", "You are not assigned to this work order, so you are not allowed to perform this action.");
    public static readonly Error NothingIsChanged = Error.Conflict("No changes detected", "There are no changes to save.");
    public static readonly Error InvoiceNotFound = Error.NotFound("The Invoice Not Found", "Invoice Not Found");
    public static readonly Error ErrorDuringGenerateInvoicePdf = Error.Failure("An error occurred while generating the invoice PDF", "An error occurred while generating the invoice PDF");
    public static readonly Error WorkOrderMustBeCompletedToIssueInvoice = Error.Validation("Work Order State Not Completed", "Issue Invoice Denied: WorkOrder Not in Complete state. Invoice can only be issued for Completed work orders");
    public static readonly Error InvoiceAlreadyIssued = Error.Conflict("The Invoice Already Issued", "Issue Invoice Cancelled: Invoice has already been issued for This WorkOrder Id ");
    public static readonly Error InvoiceIsAlreadyPaid = Error.Conflict("Invoice Already Paid", "This invoice has already been paid.");



}
