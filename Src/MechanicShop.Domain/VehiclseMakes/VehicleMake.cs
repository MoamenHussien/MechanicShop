using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

public sealed class VehicleMake : Entity
{
    public string Make { get; private set; }

    private readonly List<VehicleModel> _VehicleModels = [];

    public IReadOnlyList<VehicleModel> VehicleModels => _VehicleModels;

#pragma warning disable CS8618
    private VehicleMake()
    {
    }

#pragma warning restore CS8618
    private VehicleMake(Guid id, string Make, List<VehicleModel> vehicleModels)
        : base(id)
    {
        this.Make = Make;
        this._VehicleModels = vehicleModels;
    }

    public static Result<VehicleMake> Create(Guid id, string Make, List<VehicleModel> _vehicleModel)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(Make))
        {
            return VehicleMakeErrors.MakeRequired;
        }

        if (_vehicleModel is null || _vehicleModel.Count == 0)
        {
            return VehicleMakeErrors.ModelRequired;
        }

        return new VehicleMake(id, Make.CapitalizeFirstLetter(), _vehicleModel);
    }

    public Result<Updated> Update(string make)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleMakeErrors.MakeRequired;
        }

        this.Make = make.CapitalizeFirstLetter();

        return Result.Updated;
    }

    public Result<Updated> UpSertModels(List<VehicleModel> UpModels)
    {
        if (UpModels is null || UpModels.Count == 0)
        {
            return VehicleMakeErrors.ModelRequired;
        }

        // var Hash = UpModels.Select(n=>n.Id).ToHashSet();
        // _VehicleModels.RemoveAll(n=> !Hash.Contains(n.Id));
        var dic = _VehicleModels.ToDictionary(n => n.Id);

        foreach (var model in UpModels)
        {
            if (dic.TryGetValue(model.Id, out var vehicleModel))
            {
                var updateModelState = vehicleModel.Update(model.Model);

                if (updateModelState.IsError)
                {
                    return updateModelState.Errors;
                }
            }
            else
            {
                _VehicleModels.Add(model);
            }
        }

        return Result.Updated;
    }

    internal void Load(string make)
    {
        Make = make;
    }
}
