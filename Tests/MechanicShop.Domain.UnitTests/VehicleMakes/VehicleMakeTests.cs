using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Crypto.Engines;
using Xunit;

public class VehicleMakeTests
{
    [Fact]
    public void CreateVehicleMake_ShouldSucceed_WithValidData()
    {
        // Arrange
        Guid Id = Guid.NewGuid();
        string make = "Make-#1";
        List<VehicleModel> VehicleModels = [VehicleModelFactory.CreateVehiclModel().Value];

        // Act
        var result = VehicleMakeFactory.CreateVehicleMake(Id, make, VehicleModels);

        // Assert
        Assert.True(result.IsSuccess);
        var vehicleMake = result.Value;
        Assert.Equal(Id, vehicleMake.Id);
        Assert.Equal(make, vehicleMake.Make);
        Assert.Single(vehicleMake.VehicleModels);
        Assert.Equal(VehicleModels[0].Id, vehicleMake.VehicleModels[0].Id);
    }

    [Fact]
    public void CreateVehicleMake_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = VehicleMake.Create(Guid.Empty,  $"Make-{Guid.NewGuid().ToString().Substring(0, 8)}", [VehicleModelFactory.CreateVehiclModel().Value]);


        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }



    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateVehicleMake_ShouldFail_WhenEnterInvalidMakeName(string? make)
    {
        // Act
        var result = VehicleMake.Create(Guid.NewGuid(),  make!, [VehicleModelFactory.CreateVehiclModel().Value]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(VehicleMakeErrors.MakeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(null)]
    public void CreateVehicleMake_ShouldFail_WhenEnterInvalidModels(List<VehicleModel>? models)
    {
        // Act
        var result = VehicleMake.Create(Guid.NewGuid(),  $"Make-{Guid.NewGuid().ToString().Substring(0, 8)}", models!);


        // Assert
        Assert.True(result.IsError);
        Assert.Equal(VehicleMakeErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateVehicleMake_ShouldFail_WhenModelsListIsEmpty()
    {
        // Act
        var result = VehicleMakeFactory.CreateVehicleMake(
            _vehicleModels: []
        );

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(VehicleMakeErrors.ModelRequired.Code, result.TopError.Code);
    }

    
    [Fact]
    public void UpdateVehicleMake_ShouldUpdateMake_WhenInputIsValid()
    {
        // Arrange
        var vehicleMake = VehicleMakeFactory.CreateVehicleMake().Value;

        // Act
        var result = vehicleMake.Update("Toyota");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Toyota", vehicleMake.Make);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateVehicleMake_ShouldFail_WhenEnterInvalidMakeName(string? Make)
    {
        // Act
        var result = VehicleMakeFactory.CreateVehicleMake().Value;
        var resultofupdated = result.Update(make: Make!);

        // Assert
        Assert.True(resultofupdated.IsError);
        Assert.Equal(VehicleMakeErrors.MakeRequired.Code, resultofupdated.TopError.Code);
    }

    [Fact]
    public void UpSerVehicletModels_ShouldAddNewVehicleModelAndUpdateExisting()
    {
        // Arrange
        var olderModel = VehicleModelFactory.CreateVehiclModel(model: "M3").Value;
        var Make = VehicleMakeFactory.CreateVehicleMake(Make: "BMW", _vehicleModels: [olderModel]).Value;

        var UpdatedOlderModel = VehicleModelFactory.CreateVehiclModel(olderModel.Id, "M4").Value;
        var NewModel = VehicleModelFactory.CreateVehiclModel(model: "M5").Value;

        // Act

        var resultofupdated = Make.UpSertModels([UpdatedOlderModel, NewModel]);

        // Assert
        Assert.True(resultofupdated.IsSuccess);
        Assert.Equal(2, Make.VehicleModels.Count);
        Assert.Equal(Result.Updated, resultofupdated.Value);
        Assert.Contains(Make.VehicleModels, n => n.Id == UpdatedOlderModel.Id && n.Model == "M4");
        Assert.Contains(Make.VehicleModels, n => n.Id == NewModel.Id && n.Model == "M5");
    }

    [Fact]
    public void UpSerVehicletModels_ShouldFail_WhenModelsAreNull()
    {
        // Act
        var make = VehicleMakeFactory.CreateVehicleMake().Value;

        var result = make.UpSertModels(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(VehicleMakeErrors.ModelRequired.Code, result.TopError.Code);
    }

}