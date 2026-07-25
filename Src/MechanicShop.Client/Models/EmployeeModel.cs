namespace MechanicShop.Client.Models;

public class EmployeeModel
{
    public Guid LaborId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
