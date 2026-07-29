using MechanicShop.Tests.Common.Employees;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void SingleToDto_WhenEmployeeIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var sourceEmployee = EmployeeFactory.CreateEmployee(
            Guid.NewGuid(),
            "Moamen",
            "Hussien").Value;

        // Act
        var laborDto = sourceEmployee.ToDto();

        // Assert
        Assert.Equal(sourceEmployee.Id, laborDto.LaborId);
        Assert.Equal(sourceEmployee.FullName, laborDto.Name);
    }

    [Fact]
    public void GroupToDto_WhenEmployeesAreValid_ShouldMapAllEmployees()
    {
        // Arrange
        var firstEmployee = EmployeeFactory.CreateEmployee().Value;
        var secondEmployee = EmployeeFactory.CreateEmployee().Value;

        IList<Employee> sourceEmployees =
        [
            firstEmployee,
            secondEmployee
        ];

        // Act
        var laborDtos = sourceEmployees.ToDto();

        // Assert
        Assert.Equal(sourceEmployees.Count, laborDtos.Count);

        Assert.Contains(laborDtos, dto => dto.LaborId == firstEmployee.Id);
        Assert.Contains(laborDtos, dto => dto.LaborId == secondEmployee.Id);
    }
}
