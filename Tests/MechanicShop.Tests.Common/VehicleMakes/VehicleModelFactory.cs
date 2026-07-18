using Azure.Core;

public static class VehicleModelFactory{
    
    public static Result<VehicleModel> CreateVehiclModel(Guid? id=null ,string? model=null)
    {
        return VehicleModel.Create(id??Guid.NewGuid(),model??"Model-#1").Value;
    }
}