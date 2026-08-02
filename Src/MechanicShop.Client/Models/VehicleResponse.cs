namespace MechanicShop.Client.Models;

public class VehicleResponse
{
    public Guid Id { get; set; }

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string LicensePlate { get; set; } = string.Empty;
}
