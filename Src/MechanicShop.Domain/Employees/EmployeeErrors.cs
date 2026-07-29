public static class EmployeeErrors
{
    public static readonly Error FirstNameRequired = Error.Validation("Employee.FirstName.Required", "You Must Enter Employee First Name");
    public static readonly Error LastNameRequired = Error.Validation("Employee.LastName.Required", "You Must Enter Employee Last Name");
    public static readonly Error AlreadyInactive = Error.Validation("Employee Is Already InActive", "Employee Already InActive");


}
