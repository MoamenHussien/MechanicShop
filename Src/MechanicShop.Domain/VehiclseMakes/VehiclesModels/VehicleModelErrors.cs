public static class vehicleModelsErrors
{
    public static readonly Error ModelRequired = Error.Validation("VehicleModel.Model.Required", "You Must Enter Valid VehicleModel");
    public static readonly Error IdRequired = Error.Validation("vehicleModel.Id.Required", "You Must Enter Id");
}