using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.UpdateMake;

public class UpdateMakeCommandValidatorTests
{
    private readonly UpdateMakeCommandValidator _sut;

    public UpdateMakeCommandValidatorTests()
    {
        _sut = new UpdateMakeCommandValidator();
    }

    private static UpdateMakeCommand CreateCommand(
        Guid? id = null,
        string? make = null,
        List<UpdateModelCommand>? models = null)
    {
        return new(
            id ?? Guid.NewGuid(),
            make ?? "Toyota",
            models ??
            [
                new UpdateModelCommand(null, "Corolla")
            ]);
    }

    [Fact]
    public void UpdateMakeCommandValidator_ShouldSucceed_WithValidInputs()
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
    public void UpdateMakeCommandValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = CreateCommand(id: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateMakeCommandValidator_ShouldFail_WithInvalidMake(string? value)
    {
        // Arrange
        var command = new UpdateMakeCommand(Guid.NewGuid(), value!, [new UpdateModelCommand(null, "Corolla")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Make");
    }

    [Fact]
    public void UpdateMakeCommandValidator_ShouldFail_WithNullModels()
    {
        // Arrange
        var command = new UpdateMakeCommand(Guid.NewGuid(), "Toyota", null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Models");
    }

    [Fact]
    public void UpdateMakeCommandValidator_ShouldFail_WithEmptyModels()
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
    public void UpdateMakeCommandValidator_ShouldFail_WhenModelIsInvalid()
    {
        // Arrange
        var command = CreateCommand(models: [new UpdateModelCommand(null, "")]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Models[0].model");
    }
}
