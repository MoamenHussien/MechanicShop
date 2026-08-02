using System.Diagnostics.Metrics;
using System.Globalization;

public sealed class Vehicle : AuditableEntity
{
    public int Year { get; private set; }

    public string LicensePlate { get; private set; }

    public VehicleModel VehicleModel { get; private set; } = null!;

    public Guid VehicleModelId { get; private set; }

    public Customer Customer { get; init; } = null!;

    public Guid CustomerId { get; init; }

    public string VehicleInfo => LicensePlate + " " + Year;

    private List<WorkOrder> _workOrders = [];

    public IReadOnlyList<WorkOrder> WorkOrders => _workOrders;

#pragma warning disable CS8618
    private Vehicle()
    {
    }
#pragma warning restore CS8618

    private Vehicle(Guid id, int Year, string LicensePlate, Guid VehicleModelId)
        : base(id)
    {
        this.Year = Year;
        this.LicensePlate = LicensePlate;
        this.VehicleModelId = VehicleModelId;
    }

    public static Result<Vehicle> Create(Guid id, int Year, string LicensePlate, Guid VehicleModelId)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (Year < 1990)
        {
            return VehicleErrors.ValidVehicleYearRequired;
        }

        if (string.IsNullOrWhiteSpace(LicensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        if (VehicleModelId == Guid.Empty)
        {
            return VehicleErrors.VehicleModelRequired;
        }

        return new Vehicle(id, Year, LicensePlate.Trim(), VehicleModelId);
    }

    public Result<Updated> Update(int year, string LicensePlate, Guid VehicleModelId)
    {
        if (year < 1990)
        {
            return VehicleErrors.ValidVehicleYearRequired;
        }

        if (string.IsNullOrWhiteSpace(LicensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        if (VehicleModelId == Guid.Empty)
        {
            return VehicleErrors.VehicleModelRequired;
        }

        this.Year = year;
        this.LicensePlate = LicensePlate;
        this.VehicleModelId = VehicleModelId;

        return Result.Updated;
    }
}
