using MechanicShop.Tests.Common.Customers;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Create;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _sut;

    private static CreateCustomerCommand CreateCommand(
    string? name = null,
    string? email = null,
    string? phone = null,
    List<CreateVehicleCommand>? vehicles = null)
    {
        return new(
            name ?? "Moamen",
            email ?? "MoamenHussien25@gmail.com",
            phone ?? "+201014245762",
            vehicles ??
            [
                new CreateVehicleCommand(
                2002,
                "ABC123",
                Guid.NewGuid())
            ]);
    }

    public CreateCustomerCommandValidatorTests()
    {
        _sut = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void CreateCustomerValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData("mo")]
    [InlineData(null)]
    public void CreateCustomerValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new CreateCustomerCommand(value!, "test@test.com", "+201000000000", [new CreateVehicleCommand(2002, "ABC123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "name");
    }

    [Fact]
    public void CreateCustomerValidator_ShouldFail_WhenNameLengthGreaterThan255()
    {
        // Arrange
        var command = CreateCommand(name: new string('A', 259));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData("mo")]
    [InlineData("moamen")]
    [InlineData("moamen@")]
    [InlineData("@gmail.com")]
    [InlineData(null)]
    public void CreateCustomerValidator_ShouldFail_WithInvalidEmail(string? value)
    {
        // Arrange
        var command = new CreateCustomerCommand("Moamen", value!, "+201000000000", [new CreateVehicleCommand(2002, "ABC123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "email");
    }

    [Fact]
    public void CreateCustomerValidator_ShouldFail_WhenEmailLengthGreaterThan255()
    {
        // Arrange
        var command = CreateCommand(email: new string('A', 259) + "@gmail.com");

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("12")]
    [InlineData("random")]
    [InlineData("23541251252fd525252554545")]
    [InlineData(null)]
    public void CreateCustomerValidator_ShouldFail_WithInvalidPhoneNumber(string? value)
    {
        // Arrange
        var command = new CreateCustomerCommand("Moamen", "test@test.com", value!, [new CreateVehicleCommand(2002, "ABC123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void CreateCustomerValidator_ShouldFail_WithNullVehicles()
    {
        // Arrange
        var command = new CreateCustomerCommand("Moamen", "MoamenHussien25@gmail.com", "+201014245762", null!);

        // Act
        var result = _sut.Validate(command);

        System.Console.WriteLine($"COMMAND VEHICLES IS NULL? {command.Vehicles == null}");
        System.Console.WriteLine($"IS VALID? {result.IsValid}");
        foreach (var error in result.Errors)
        {
            System.Console.WriteLine($"ERROR: {error.PropertyName} - {error.ErrorMessage}");
        }

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Vehicles");
    }

    [Fact]
    public void CreateCustomerValidator_ShouldFail_WithEmptyVehicles()
    {
        // Arrange
        var command = CreateCommand(vehicles: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Vehicles");
    }

    [Fact]
    public void CreateCustomerValidator_ShouldFail_WhenVehicleYearIsInvalid()
    {
        // Arrange
        var command = CreateCommand(vehicles: [new CreateVehicleCommand(1900, "ABC123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Vehicles[0].year");
    }
}
