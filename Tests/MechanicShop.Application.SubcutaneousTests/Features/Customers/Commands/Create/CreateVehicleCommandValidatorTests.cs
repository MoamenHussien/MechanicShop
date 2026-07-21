using Xunit;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _sut;

    private static readonly Guid VehicleModelId =
        "11111111-1111-1111-1111-222222222221".ToGuid().Value;

    public CreateVehicleCommandValidatorTests()
    {
        _sut = new CreateVehicleCommandValidator();
    }

    private static CreateVehicleCommand CreateCommand(
        int? year = null,
        string? licensePlate = null,
        Guid? vehicleModelId = null)
    {
        return new(
            year ?? 2000,
            licensePlate ?? "ABC123",
            vehicleModelId ?? VehicleModelId);
    }

    [Fact]
    public void CreateVehicleCommandValidator_ShouldSucceed_WithValidData()
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
    [InlineData(21)]
    [InlineData(1900)]
    public void CreateVehicleCommandValidator_ShouldFail_WithInvalidYear(int value)
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
    [InlineData("df")]
    [InlineData("fdfsfsdfs3232")]
    public void CreateVehicleCommandValidator_ShouldFail_WithInvalidLicensePlate(string value)
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
    public void CreateVehicleCommandValidator_ShouldFail_WithEmptyVehicleModelId()
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