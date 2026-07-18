using MechanicShop.Tests.Common.Employees;
using Xunit;

public class EmployeeTests
{
    [Fact]
    public void CreateEmployee_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";

        // Act
        var result = EmployeeFactory.CreateEmployee(
            id: id,
            firstName: firstName,
            lastName: lastName);

        // Assert
        Assert.True(result.IsSuccess);

        var employee = result.Value;

        Assert.Equal(id, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.True(employee.IsActive);
        Assert.Equal("John Doe", employee.FullName);
    }

    [Fact]
    public void CreateEmployee_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = EmployeeFactory.CreateEmployee(id: Guid.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateEmployee_ShouldFail_WithInvalidFirstName(string? value)
    {
        // Act
        var result = EmployeeFactory.CreateEmployee(firstName: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateEmployee_ShouldFail_WithInvalidLastName(string? value)
    {
        // Act
        var result = EmployeeFactory.CreateEmployee(lastName: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.LastNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateEmployee_ShouldSucceed_WithValidData()
    {
        // Arrange
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Act
        var result = employee.Update(
            FirstName: "Ahmed",
            LastName: "Ali",
            IsActive: false);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("Ahmed", employee.FirstName);
        Assert.Equal("Ali", employee.LastName);
        Assert.False(employee.IsActive);
        Assert.Equal("Ahmed Ali", employee.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateEmployee_ShouldFail_WithInvalidFirstName(string? value)
    {
        // Arrange
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Act
        var result = employee.Update(
            FirstName: value!,
            LastName: "Ali",
            IsActive: true);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateEmployee_ShouldFail_WithInvalidLastName(string? value)
    {
        // Arrange
        var employee = EmployeeFactory.CreateEmployee().Value;

        // Act
        var result = employee.Update(
            FirstName: "Ahmed",
            LastName: value!,
            IsActive: true);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.LastNameRequired.Code, result.TopError.Code);
    }
}