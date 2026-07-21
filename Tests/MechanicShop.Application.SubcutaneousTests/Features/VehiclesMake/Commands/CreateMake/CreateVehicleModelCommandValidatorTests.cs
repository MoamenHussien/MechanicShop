using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.CreateMake;

public class CreateVehicleModelCommandValidatorTests
{
    private readonly CreateVehicleModelCommandValidator _sut;

    public CreateVehicleModelCommandValidatorTests()
    {
        _sut = new CreateVehicleModelCommandValidator();
    }

    private static CreateVehicleModelCommand CreateCommand(string? model = null)
    {
        return new(model ?? "Corolla");
    }

    [Fact]
    public void CreateVehicleModelCommandValidator_ShouldSucceed_WithValidData()
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
    public void CreateVehicleModelCommandValidator_ShouldFail_WithInvalidModel(string? value)
    {
        // Arrange
        var command = new CreateVehicleModelCommand(value!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "model");
    }
}
