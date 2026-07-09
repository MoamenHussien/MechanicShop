public class CustomerDto
{
    public Guid Id { get; init;  }
    public string Name { get; init; }=null!;
    public string Email { get; init;  } =null!;
    public string  PhoneNumber { get; init;  } =null!;
    public List< VehicleDto> vehicles  { get;init;  } =null!;
}