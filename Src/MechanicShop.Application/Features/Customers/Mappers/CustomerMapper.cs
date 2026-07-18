using System.Linq.Expressions;

public static class CustomerMapper
{
    public static readonly Expression<Func<Customer, CustomerDto>> CustomerProjection =
        customer => new CustomerDto
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Vehicles = customer.vehicles
                .Select(vehicle => new VehicleDto(
                    vehicle.Id,
                    vehicle.VehicleModel.VehicleMake.Make,
                    vehicle.VehicleModel.Model,
                    vehicle.Year,
                    vehicle.LicensePlate))
                .ToList()
        };

    public static readonly Expression<Func<Vehicle, VehicleDto>> VehicleProjection =
        vehicle => new VehicleDto(
            vehicle.Id,
            vehicle.VehicleModel.VehicleMake.Make,
            vehicle.VehicleModel.Model,
            vehicle.Year,
            vehicle.LicensePlate);

    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new CustomerDto
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Vehicles = customer.vehicles.ToDto()
        };
    }

    public static VehicleDto ToDto(this Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(vehicle.VehicleModel);
        ArgumentNullException.ThrowIfNull(vehicle.VehicleModel.VehicleMake);

        return new VehicleDto(
            vehicle.Id,
            vehicle.VehicleModel.VehicleMake.Make,
            vehicle.VehicleModel.Model,
            vehicle.Year,
            vehicle.LicensePlate);
    }

    public static List<CustomerDto> ToDto(this IEnumerable<Customer> customers)
    {
        return customers.Select(customer => customer.ToDto()).ToList();
    }

    public static List<VehicleDto> ToDto(this IEnumerable<Vehicle> vehicles)
    {
        return vehicles.Select(vehicle => vehicle.ToDto()).ToList();
    }
}