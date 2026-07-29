public static class VehiclesMakeMapper
{
    public static VehicleMakeDto ToMakeDto(this VehicleMake make)
    {
        ArgumentNullException.ThrowIfNull(make);
        return new VehicleMakeDto
        {
            Id = make.Id,
            Make = make.Make,
            VehiclesModels = make.VehicleModels.ToModelsDto()
        };
    }

    public static List<VehicleMakeDto> ToMakesDto(this IEnumerable<VehicleMake> vehicleMakes)
    {
        return vehicleMakes.Select(n => ToMakeDto(n)).ToList();
    }

    public static VehicleModelDto ToModelDto(this VehicleModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new VehicleModelDto(model.Id, model.Model);
    }

    public static List<VehicleModelDto> ToModelsDto(this IEnumerable<VehicleModel> Models)
    {
        return Models.Select(n => ToModelDto(n)).ToList();
    }
}
