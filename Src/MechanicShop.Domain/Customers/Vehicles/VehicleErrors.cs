public static class VehicleErrors
{
    public static readonly Error ValidVehicleYearRequired = Error.Validation("Enter Valid Year Number", "The Vehicle Year Is Wrong Must > Than 1990");
    public static readonly Error LicensePlateRequired = Error.Validation("Vehicle.LicensePlate.Required,", "You Must Enter Vehicle LicensePlate");
    public static readonly Error VehicleModelRequired = Error.Validation("Vehicle.Model.Required,", "You Must Enter Vehicle Model");
}
