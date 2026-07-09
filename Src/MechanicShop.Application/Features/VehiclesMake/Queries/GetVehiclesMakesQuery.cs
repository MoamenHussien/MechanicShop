using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

public sealed record GetVehiclesMakesQuery : ICachedQuery<Result<List<VehicleMakeResponse>>>
{
    public string CacheKey => "VehiclesMakes";

    public string[] Tags => ["VMakes"];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}


public class GetVehiclesMakesQueriesHandler(IAppDbContext context , ILogger<GetVehiclesMakesQueriesHandler> logger)
 : IRequestHandler<GetVehiclesMakesQuery, Result<List<VehicleMakeResponse>>>
{
    async Task<Result<List<VehicleMakeResponse>>> IRequestHandler<GetVehiclesMakesQuery, Result<List<VehicleMakeResponse>>>.Handle(GetVehiclesMakesQuery request, CancellationToken cancellationToken)
    {
       var makes = await context.VehicleMakes.AsNoTracking().Select(x=> new VehicleMakeResponse(x.Id,x.Make)).ToListAsync(cancellationToken);
       if (makes.Count > 0 )
        {
           logger.LogWarning("Not Found Any Of Vehicles Makes");
           return  ApplicationErrors.NotFoundAnyMakes; 
        }

        logger.LogInformation("Returning And Caching Vehicles Makes");

        return  makes;
    }
}