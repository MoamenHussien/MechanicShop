using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MechanicShop.Application.Common.Constants;

public sealed record GetLaborsQuery() : ICachedQuery<Result<List<LaborDto>>>
{
    public string CacheKey => "Labors";

    public string[] Tags => [CacheTags.Users];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetLaborsQueryHandler(ILogger<GetLaborsQueryHandler> logger, IAppDbContext context, IIdentityService identity)
: IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var laborIds = await identity.GetIdsOfUsersByRoleTypeAsync(Role.Labor);
        if (laborIds.IsError)
        {
            logger.LogWarning("Failed to retrieve labor user IDs. Errors: {@Errors}", laborIds.Errors);
            return laborIds.Errors;
        }

        var labors = await context.Employees
        .AsNoTracking()
        .Where(x => x.IsActive && laborIds.Value.Contains(x.Id))
        .Select(x => x.ToDto())
        .ToListAsync(cancellationToken);

        if (!labors.Any())
        {
            logger.LogWarning("No active labor employees were found for role '{Role}'.",Role.Labor);
            return ApplicationErrors.NotFoundAnyLabors;
        }

        logger.LogInformation("Successfully retrieved {Count} active labor(s).", labors.Count);

        return labors;
    }
}