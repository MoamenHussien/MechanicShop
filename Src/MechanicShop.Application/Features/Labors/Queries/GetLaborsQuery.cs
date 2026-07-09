using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetLaborsQuery() : ICachedQuery<Result<List<LaborDto>>>
{
    public string CacheKey => "Labors";

    public string[] Tags => ["Labors"];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetLaborsQueryHandler(ILogger<GetLaborsQueryHandler> logger, IAppDbContext context,IIdentityService identity)
: IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var LaborsIds = await identity.GetIdsOfUsersByRoleTypeAsync(Role.Labor);
        if (LaborsIds.IsError)
        {
            return LaborsIds.Errors;
        }

        var Labors = await context.Employees.AsNoTracking().Where(n=> LaborsIds.Value.Contains(n.Id)).Select(n=>n.ToDto()).ToListAsync();

        if (Labors is null)
        {
            logger.LogWarning("Not Found Any Of Labors");
            return  ApplicationErrors.NotFoundAnyLabors;
        }

        logger.LogInformation("Return Successfully All Labors");

        return Labors;
    }
}