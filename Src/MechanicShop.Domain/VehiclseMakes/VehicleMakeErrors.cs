public static class VehicleMakeErrors
{
    public static readonly Error IdRequired = Error.Validation("VehicleMake.Id.Required", "You Must Enter Make Id");
    public static readonly Error MakeRequired = Error.Validation("VehicleMake.Make.Required", "VehicleMake Name Is Required");
    public static readonly Error ModelRequired = Error.Validation("VehicleMake.Models.Required", "You Must Enter At Least One Model");
    public static readonly Error MakeIsAlreadyExists = Error.Conflict("VehicleMake.Make.Exists", "This Vehicle Make already exists");
}
