
using System.Reflection;
namespace MechanicShop.Tests.Common.Customers;

public static class VehicleFactory
{
    public static Result<Vehicle> CreateVehicle(
        Guid? id = null,
        Guid? vehicleModelId = null,
        int? year = null,
        string? licensePlate = null,
        VehicleModel? vehicleModel = null)
    {
        var result = Vehicle.Create(
            id ?? Guid.NewGuid(),
            year ?? 2024,
            licensePlate ?? "ABC 123",
            vehicleModelId ?? vehicleModel?.Id ?? "11111111-1111-1111-1111-222222222221".ToGuid().Value);

        if (result.IsError)
        {
            return result;
        }

        if (vehicleModel is not null)
        {
            SetPrivateProperty(result.Value, nameof(Vehicle.VehicleModel), vehicleModel);
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
