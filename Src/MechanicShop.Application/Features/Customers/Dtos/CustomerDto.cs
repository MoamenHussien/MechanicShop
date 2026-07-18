public class CustomerDto
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public List<VehicleDto> Vehicles { get; init; } = null!;
}