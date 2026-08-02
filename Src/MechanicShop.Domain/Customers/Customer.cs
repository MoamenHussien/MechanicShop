using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

public sealed class Customer : AuditableEntity
{
    public string Name { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; private set; }

    private readonly List<Vehicle> _vehicles = [];

    public IReadOnlyCollection<Vehicle> vehicles => _vehicles;

#pragma warning disable CS8618
    private Customer()
    {
    }

#pragma warning restore CS8618

    private Customer(Guid id, string name, string email, string phone, List<Vehicle> vehicles)
        : base(id)
    {
        this.Name = name;
        this.Email = email;
        this.PhoneNumber = phone;
        this._vehicles = vehicles;
    }

    public static Result<Customer> Create(Guid id, string name, string email, string phone, List<Vehicle> vehicles)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return CustomerErrors.PhoneRequired;
        }

        if (vehicles is null || vehicles.Count == 0)
        {
            return CustomerErrors.VehiclesRequired;
        }

        return new Customer(id, name.CapitalizeFirstLetter(), email, phone.Trim(), vehicles);
    }

    public Result<Updated> Update(string name, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CustomerErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CustomerErrors.EmailRequired;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return CustomerErrors.PhoneRequired;
        }

        this.Name = name.CapitalizeFirstLetter();
        this.Email = email;
        this.PhoneNumber = phone.Trim();

        return Result.Updated;
    }

    public Result<Updated> UpSertVehicles(List<Vehicle> vehicles)
    {
        if (vehicles is null || vehicles.Count == 0)
        {
            return CustomerErrors.VehiclesRequired;
        }

        var vehicleHash = vehicles.Select(n => n.Id).ToHashSet();

        this._vehicles.RemoveAll(n => !vehicleHash.Contains(n.Id));

        var vehicleDire = this._vehicles.ToDictionary(n => n.Id);

        foreach (var upVec in vehicles)
        {
            if (vehicleDire.TryGetValue(upVec.Id, out Vehicle? vehicle))
            {
                var upState = vehicle.Update(upVec.Year, upVec.LicensePlate, upVec.VehicleModelId);
                if (upState.IsError)
                {
                    return upState.Errors;
                }
            }
            else
            {
                this._vehicles.Add(upVec);
            }
        }

        return Result.Updated;
    }
}
