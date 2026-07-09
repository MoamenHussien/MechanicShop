using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

public sealed class VehicleMake : Entity
{
    public string Make { get; private set; }
    private readonly List<VehicleModel> _VehicleModel =[];
    public IReadOnlyList<VehicleModel> VehicleModels => _VehicleModel;

#pragma warning disable CS8618
private VehicleMake()
{
    
}

#pragma warning restore CS8618
    private VehicleMake(Guid id, string Make,List<VehicleModel> vehicleModels) : base(id)
    {
        this.Make = Make;
        this._VehicleModel = vehicleModels;
    }

    public static Result<VehicleMake> Create(Guid id , string Make, List<VehicleModel> _vehicleModel)
    {
        if (id == Guid.Empty)
        {
           return VehicleMakeErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(Make))
        {
           return VehicleMakeErrors.MakeRequired;
        }

        if (_vehicleModel is null || _vehicleModel.Count() < 0)
        {
            return VehicleMakeErrors.ModelRequired;
        }

        return new VehicleMake(id,Make.CapitalizeFirstLetter(),_vehicleModel);
    }

     public Result<Updated> Update(string make)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
           return VehicleMakeErrors.MakeRequired;
        }

        this.Make=make.CapitalizeFirstLetter();

       return Result.Updated;
    }

    public Result<Updated> UpSertModels(List<VehicleModel> UpModels)
    {
        if (UpModels is null || UpModels.Count() < 0)
        {
            return VehicleMakeErrors.ModelRequired;
        }

        var Hash = UpModels.Select(n=>n.Id).ToHashSet();
        _VehicleModel.RemoveAll(n=> !Hash.Contains(n.Id));

        var Dic = _VehicleModel.ToDictionary(n=>n.Id);

        foreach(var model in UpModels)
        {
            if (Dic.TryGetValue(model.Id,out var vehicleModel))
            {
                var UpdateModelState = vehicleModel.Update(model.Model);

                if (UpdateModelState.IsError)
                {
                    return UpdateModelState.Errors;
                }
            }
            else
            {
                _VehicleModel.Add(model);
            }  
        }
        return Result.Updated;
    }













    
}