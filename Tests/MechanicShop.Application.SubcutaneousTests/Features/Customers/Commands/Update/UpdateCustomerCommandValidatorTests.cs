using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Update;

public class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _sut;

    public UpdateCustomerCommandValidatorTests()
    {
        _sut = new UpdateCustomerCommandValidator();
    }

    private static UpdateCustomerCommand CreateCommand(
        Guid? id = null,
        string? name = null,
        string? email = null,
        string? phoneNumber = null,
        List<UpdateVehicleCommand>? vehicles = null)
    {
        return new UpdateCustomerCommand(
            id ?? Guid.NewGuid(),
            name ?? "Valid Name",
            email ?? "valid@example.com",
            phoneNumber ?? "+201012345678",
            vehicles ?? [new UpdateVehicleCommand(Guid.NewGuid(), 2020, "ABC-123", Guid.NewGuid())]);
    }

    [Fact]
    public void UpdateCustomerValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateCustomerValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = CreateCommand(id: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")] // length < 3
    [InlineData(null)]
    public void UpdateCustomerValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new UpdateCustomerCommand(Guid.NewGuid(), value!, "valid@example.com", "+201012345678", [new UpdateVehicleCommand(Guid.NewGuid(), 2020, "ABC-123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Fact]
    public void UpdateCustomerValidator_ShouldFail_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var command = CreateCommand(name: new string('A', 256));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData(null)]
    public void UpdateCustomerValidator_ShouldFail_WithInvalidEmail(string? value)
    {
        // Arrange
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Valid Name", value!, "+201012345678", [new UpdateVehicleCommand(Guid.NewGuid(), 2020, "ABC-123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-phone")]
    [InlineData(null)]
    public void UpdateCustomerValidator_ShouldFail_WithInvalidPhone(string? value)
    {
        // Arrange
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Valid Name", "valid@example.com", value!, [new UpdateVehicleCommand(Guid.NewGuid(), 2020, "ABC-123", Guid.NewGuid())]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void UpdateCustomerValidator_ShouldFail_WithNullVehicles()
    {
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Valid Name", "valid@example.com", "+201012345678", null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Vehicles");
    }

    [Fact]
    public void UpdateCustomerValidator_ShouldFail_WithEmptyVehicles()
    {
        // Arrange
        var command = CreateCommand(vehicles: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Vehicles");
    }
}
