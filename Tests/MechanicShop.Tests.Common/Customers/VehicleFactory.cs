
namespace MechanicShop.Tests.Common.Customers;

public static class VehicleFactory
{
    public static Result<Vehicle> CreateVehicle(Guid? id = null, Guid? vehicleModelId = null, int? year = null, string? licensePlate = null)
    {
        return Vehicle.Create(
            id ?? Guid.NewGuid(),
            year ?? 2024,
            licensePlate ?? "ABC 123",
            vehicleModelId ?? "11111111-1111-1111-1111-222222222221".ToGuid().Value
            );
    }
}