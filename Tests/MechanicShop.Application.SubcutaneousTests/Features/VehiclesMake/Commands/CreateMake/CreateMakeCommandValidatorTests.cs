using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.CreateMake;

public class CreateMakeCommandValidatorTests
{
    private readonly CreateMakeCommandValidator _sut;

    public CreateMakeCommandValidatorTests()
    {
        _sut = new CreateMakeCommandValidator();
    }

    private static CreateMakeCommand CreateCommand(
        string? make = null,
        List<CreateVehicleModelCommand>? models = null)
    {
        return new(
            make ?? "Toyota",
            models ??
            [
                new CreateVehicleModelCommand("Corolla")
            ]);
    }

    [Fact]
    public void CreateMakeCommandValidator_ShouldSucceed_WithValidInputs()
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
    [InlineData(null)]
    public void CreateMakeCommandValidator_ShouldFail_WithInvalidMake(string? value)
    {
        // Arrange
        var command = new CreateMakeCommand(value!, [new CreateVehicleModelCommand("Corolla")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Make");
    }

    [Fact]
    public void CreateMakeCommandValidator_ShouldFail_WithNullModels()
    {
        // Arrange
        var command = new CreateMakeCommand("Toyota", null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Models");
    }

    [Fact]
    public void CreateMakeCommandValidator_ShouldFail_WithEmptyModels()
    {
        // Arrange
        var command = CreateCommand(models: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Models");
    }

    [Fact]
    public void CreateMakeCommandValidator_ShouldFail_WhenModelIsInvalid()
    {
        // Arrange
        var command = CreateCommand(models: [new CreateVehicleModelCommand("")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Models[0].model");
    }
}
