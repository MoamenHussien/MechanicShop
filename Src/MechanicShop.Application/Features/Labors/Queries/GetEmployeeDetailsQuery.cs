using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Features.Labors.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Labors.Queries;

public sealed record GetEmployeeDetailsQuery() : ICachedQuery<Result<List<EmployeeDetailDto>>>
{
    public string CacheKey => "EmployeeDetails";

    public string[] Tags => [CacheTags.Users];

    public TimeSpan Expiration => TimeSpan.FromDays(1);
}

public class GetEmployeeDetailsQueryHandler(
    ILogger<GetEmployeeDetailsQueryHandler> logger,
    IIdentityService identity)
    : IRequestHandler<GetEmployeeDetailsQuery, Result<List<EmployeeDetailDto>>>
{
    public async Task<Result<List<EmployeeDetailDto>>> Handle(GetEmployeeDetailsQuery request, CancellationToken cancellationToken)
    {
        var result = await identity.GetEmployeeDetailsAsync(cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Failed to retrieve employee details. Errors: {@Errors}", result.Errors);
            return result.Errors;
        }

        logger.LogInformation("Successfully retrieved {Count} employee detail record(s)", result.Value.Count);
        return result.Value;
    }
}
