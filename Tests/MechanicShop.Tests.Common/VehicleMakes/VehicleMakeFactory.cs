public static class VehicleMakeFactory
{
    public static Result<VehicleMake> CreateVehicleMake(Guid? id = null, string? Make = null, List<VehicleModel>? _vehicleModels = null)
    {
        return VehicleMake.Create(id ?? Guid.NewGuid(), Make ?? $"Make-{Guid.NewGuid().ToString().Substring(0, 8)}", _vehicleModels ?? [VehicleModelFactory.CreateVehiclModel().Value]);
    }
}
