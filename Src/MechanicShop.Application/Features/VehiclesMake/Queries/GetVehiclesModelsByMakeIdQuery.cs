using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

public sealed record GetVehiclesModelsByMakeIdQuery(Guid MakeId) : ICachedQuery<Result<List<VehicleModelDto>>>
{
    public string CacheKey => $"ModelsFor{MakeId}";

    public string[] Tags => ["VMakes"];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetVehiclesModelsByMakeIdQueryValidator : AbstractValidator<GetVehiclesModelsByMakeIdQuery>
{
    public GetVehiclesModelsByMakeIdQueryValidator()
    {
        RuleFor(n=>n.MakeId).IdRequired("Make");
    }
}

public class GetVehiclesModelsByMakeIdQueryHandler(IAppDbContext context, ILogger<GetVehiclesModelsByMakeIdQueryHandler> logger)
: IRequestHandler<GetVehiclesModelsByMakeIdQuery, Result<List<VehicleModelDto>>>
{
    public async Task<Result<List<VehicleModelDto>>> Handle(GetVehiclesModelsByMakeIdQuery request, CancellationToken cancellationToken)
    {
        var models = await context.VehicleModels.AsNoTracking().Where(x=>x.VehicleMakeId==request.MakeId)
                                                .Select(x=>new VehicleModelDto(x.Id,x.Model)).ToListAsync(cancellationToken);
        if (models.Count == 0)
        {
          logger.LogWarning("Not Found Any Models To This Make Id : {id}",request.MakeId);
          return  ApplicationErrors.NotFoundAnyModelsToThisMakeId;
        }

        logger.LogInformation("Returning And Caching Vehicles Models To This Make Id : {id}",request.MakeId);

        return models;

    }
}