using Xunit;

public class VehicleModelTests
{

    [Fact]
    public void CreateVehiclModel_ShouldSucceed_WithValidData()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string model = "M4";

        // Act
        var Vehiclemodel = VehicleModelFactory.CreateVehiclModel(id, model);

        // Assert
        Assert.True(Vehiclemodel.IsSuccess);
        Assert.Equal(id, Vehiclemodel.Value.Id);
        Assert.Equal(model, Vehiclemodel.Value.Model);
    }

    [Fact]
    public void CreateVehicleModel_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = VehicleModel.Create(id:Guid.Empty,"Model-#1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateVehicleModel_ShouldFail_WithInvalidModelName(string? value)
    {
        // Act
        var result = VehicleModel.Create(id:Guid.NewGuid(),value!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(vehicleModelsErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateVehicleModel_ShouldSucceed_WithValidData()
    {
        // Arrange
        var olderModel = VehicleModelFactory.CreateVehiclModel(model: "m3").Value;

        // Act
        var result = olderModel.Update("m4");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("M4", olderModel.Model);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateVehicleModel_ShouldFail_WithInvalidModleName(string? value)
    {
        // Act
        var result = VehicleModelFactory.CreateVehiclModel(model: "M3").Value;
        var resultofupdated = result.Update(value!);

        // Assert
        Assert.True(resultofupdated.IsError);
        Assert.Equal(vehicleModelsErrors.ModelRequired.Code, resultofupdated.TopError.Code);
    }


}

