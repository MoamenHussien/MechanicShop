using MechanicShop.Tests.Common.Customers;
using Xunit;

public class VehicleTests
{
    [Fact]
    public void CreateVehicle_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var vehicleModelId = Guid.NewGuid();
        const int year = 2024;
        const string licensePlate = "ABC123";

        // Act
        var result = VehicleFactory.CreateVehicle(
            id: id,
            year: year,
            licensePlate: licensePlate,
            vehicleModelId: vehicleModelId);

        // Assert
        Assert.True(result.IsSuccess);

        var vehicle = result.Value;

        Assert.Equal(id, vehicle.Id);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(licensePlate, vehicle.LicensePlate);
        Assert.Equal(vehicleModelId, vehicle.VehicleModelId);
    }

    [Fact]
    public void CreateVehicle_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = VehicleFactory.CreateVehicle(id: Guid.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(1989)]
    [InlineData(1800)]
    public void CreateVehicle_ShouldFail_WithInvalidYear(int value)
    {
        // Act
        var result = VehicleFactory.CreateVehicle(year: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.ValidVehicleYearRequired.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateVehicle_ShouldFail_WithInvalidLicensePlate(string? value)
    {
        // Act
        var result = VehicleFactory.CreateVehicle(licensePlate: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.LicensePlateRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void CreateVehicle_ShouldFail_WithInvalidVehicleModelId()
    {
        // Act
        var result = VehicleFactory.CreateVehicle(id: Guid.Empty);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.VehicleModelRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpdateVehicle_ShouldSucceed_WithValidData()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var vehicleModelId = Guid.NewGuid();

        // Act
        var result = vehicle.Update(
            year: 2023,
            LicensePlate: "XYZ123",
            VehicleModelId: vehicleModelId);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(2023, vehicle.Year);
        Assert.Equal("XYZ123", vehicle.LicensePlate);
        Assert.Equal(vehicleModelId, vehicle.VehicleModelId);
    }

    [Theory]
    [InlineData(1989)]
    [InlineData(1800)]
    public void UpdateVehicle_ShouldFail_WithInvalidYear(int value)
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;

        // Act
        var result = vehicle.Update(
            year: value,
            LicensePlate: "XYZ123",
            VehicleModelId: Guid.NewGuid());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.ValidVehicleYearRequired.Code,
            result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateVehicle_ShouldFail_WithInvalidLicensePlate(string? value)
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;

        // Act
        var result = vehicle.Update(
            year: 2023,
            LicensePlate: value!,
            VehicleModelId: Guid.NewGuid());

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.LicensePlateRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WithInvalidVehicleModelId()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;

        // Act
        var result = vehicle.Update(
            year: 2023,
            LicensePlate: "XYZ123",
            VehicleModelId: Guid.Empty);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            VehicleErrors.VehicleModelRequired.Code,
            result.TopError.Code);
    }
}