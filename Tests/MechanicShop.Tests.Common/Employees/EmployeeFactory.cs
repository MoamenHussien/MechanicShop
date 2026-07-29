
namespace MechanicShop.Tests.Common.Employees;

public static class EmployeeFactory
{
    public static Result<Employee> CreateEmployee(Guid? id = null, string? firstName = null, string? lastName = null)
    {
        return Employee.Create(
            id ?? Guid.NewGuid(),
            firstName ?? "John",
            lastName ?? "Doe"
            );
    }
}
