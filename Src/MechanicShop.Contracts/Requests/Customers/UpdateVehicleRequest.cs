using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;

public class UpdateVehicleRequest
{
    public Guid? VehicleId { get; set; }

    [Required(ErrorMessage = "Model is required.")]
    public Guid ModelId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Year is required.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Spot is required.")]
    public string LicensePlate { get; set; } = string.Empty;
}
