using System.Reflection;

public static class VehicleModelFactory
{
    public static Result<VehicleModel> CreateVehiclModel(
        Guid? id = null,
        string? model = null,
        VehicleMake? vehicleMake = null)
    {
        var result = VehicleModel.Create(
            id ?? Guid.NewGuid(),
            model ?? "Corolla");

        if (result.IsError)
        {
            return result;
        }

        if (vehicleMake is not null)
        {
            SetPrivateProperty(result.Value, nameof(VehicleModel.VehicleMake), vehicleMake);
        }

        return result;
    }

    private static void SetPrivateProperty<T>(
        object target,
        string propertyName,
        T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic);

        property!.SetValue(target, value);
    }
}
