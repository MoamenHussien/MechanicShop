public static class VehicleMakeFactory
{

    public static Result<VehicleMake> CreateVehicleMake(Guid? id = null, string? Make = null, List<VehicleModel>? _vehicleModels = null)
    {
        return VehicleMake.Create(id ?? Guid.NewGuid(), Make ?? "Make-#1", _vehicleModels ?? [VehicleModelFactory.CreateVehiclModel().Value]).Value;
    }

}