using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetLaborsQuery() : ICachedQuery<Result<List<LaborDto>>>
{
    public string CacheKey => "Labors";

    public string[] Tags => ["Labors"];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetLaborsQueryHandler(ILogger<GetLaborsQueryHandler> logger, IAppDbContext context, IIdentityService identity)
: IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var LaborsIds = await identity.GetIdsOfUsersByRoleTypeAsync(Role.Labor);
        if (LaborsIds.IsError)
        {
            logger.LogWarning("Failed to retrieve labor user IDs. Errors: {@Errors}", LaborsIds.Errors);
            return LaborsIds.Errors;
        }

        var labors = await context.Employees
        .AsNoTracking()
        .Where(x => x.IsActive && LaborsIds.Value.Contains(x.Id))
        .Select(x => x.ToDto())
        .ToListAsync(cancellationToken);

        if (labors.Any())
        {
            logger.LogWarning("Not Found Any Of Labors");
            return ApplicationErrors.NotFoundAnyLabors;
        }

        logger.LogInformation("Successfully retrieved {Count} active labor(s).", labors.Count);

        return labors;
    }
}