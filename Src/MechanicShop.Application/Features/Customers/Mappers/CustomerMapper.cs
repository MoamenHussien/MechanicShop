public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return new CustomerDto
        {
            Id=customer.Id,
            Name=customer.Name,
            Email=customer.Email,
            PhoneNumber=customer.PhoneNumber,
            vehicles=customer.vehicles.ToDto()

        };
    }

    public static VehicleDto ToDto(this Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        return new VehicleDto(vehicle.Id,vehicle.Year,vehicle.LicensePlate,vehicle.VehicleModelId);
    }

    public static List<VehicleDto> ToDto(this IEnumerable<Vehicle> vehicles)
    {
        return vehicles.Select(n=>n.ToDto()).ToList();
    }

    public static List<CustomerDto> ToDto(this IEnumerable<Customer> customers)
    {
        return customers.Select(n=>n.ToDto()).ToList();
    }
}