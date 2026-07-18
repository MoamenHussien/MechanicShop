using System.Data.Common;
using System.Net.Http.Headers;

public sealed class VehicleModel : Entity
{
    public string Model { get; private set; }
    public VehicleMake VehicleMake { get; init; } = null!;
    public Guid VehicleMakeId { get; init; }
    private readonly List<Vehicle> _vehicles = [];
    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

#pragma warning disable CS8618
    private VehicleModel()
    {
    }

#pragma warning restore CS8618

    private VehicleModel(Guid id, string Model) : base(id)
    {
        this.Model = Model;
    }

    public static Result<VehicleModel> Create(Guid id, string model)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return vehicleModelsErrors.ModelRequired;
        }

        return new VehicleModel(id, model.CapitalizeFirstLetter());
    }

    public Result<Updated> Update(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return vehicleModelsErrors.ModelRequired;
        }
        this.Model = model.CapitalizeFirstLetter();

        return Result.Updated;
    }

    internal void Load(string model, string make)
    {
        Model = model;
        VehicleMake.Load(make);
    }

}




