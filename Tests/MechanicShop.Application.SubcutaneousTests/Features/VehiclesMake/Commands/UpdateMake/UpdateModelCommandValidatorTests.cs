using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.UpdateMake;

public class UpdateModelCommandValidatorTests
{
    private readonly UpdateModelCommandValidator _sut;

    public UpdateModelCommandValidatorTests()
    {
        _sut = new UpdateModelCommandValidator();
    }

    private static UpdateModelCommand CreateCommand(
        Guid? modelId = null,
        string? model = null)
    {
        return new(
            modelId,
            model ?? "Corolla");
    }

    [Fact]
    public void UpdateModelCommandValidator_ShouldSucceed_WithValidData()
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
    public void UpdateModelCommandValidator_ShouldFail_WithInvalidModel(string? value)
    {
        // Arrange
        var command = new UpdateModelCommand(Guid.NewGuid(), value!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "model");
    }
}
