using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Update;

public class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _sut;

    private static readonly Guid VehicleModelId =
        "11111111-1111-1111-1111-222222222221".ToGuid().Value;

    public UpdateVehicleCommandValidatorTests()
    {
        _sut = new UpdateVehicleCommandValidator();
    }

    private static UpdateVehicleCommand CreateCommand(
        Guid? id = null,
        int? year = null,
        string? licensePlate = null,
        Guid? vehicleModelId = null)
    {
        return new(
            id,
            year ?? 2000,
            licensePlate ?? "ABC123",
            vehicleModelId ?? VehicleModelId);
    }

    [Fact]
    public void UpdateVehicleCommandValidator_ShouldSucceed_WithValidData()
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
    [InlineData(1990)]
    [InlineData(1900)]
    [InlineData(0)]
    public void UpdateVehicleCommandValidator_ShouldFail_WithInvalidYear(int value)
    {
        // Arrange
        var command = CreateCommand(year: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "year");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData("df")] // Length 2
    [InlineData("fdfsfsdfs3232")] // Length > 8
    public void UpdateVehicleCommandValidator_ShouldFail_WithInvalidLicensePlate(string value)
    {
        // Arrange
        var command = CreateCommand(licensePlate: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "LicensePlate");
    }

    [Fact]
    public void UpdateVehicleCommandValidator_ShouldFail_WithEmptyVehicleModelId()
    {
        // Arrange
        var command = CreateCommand(vehicleModelId: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "VehicleModelId");
    }
}
