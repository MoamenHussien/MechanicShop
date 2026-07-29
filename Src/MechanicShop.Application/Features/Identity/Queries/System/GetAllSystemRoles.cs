using MediatR;
using MechanicShop.Application.Common.Constants;

public record GetAllSystemRolesQuery() : ICachedQuery<Result<List<string>>>
{
    public string CacheKey => "SystemRoles";

    public string[] Tags => [CacheTags.SystemRoles];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public sealed class GetAllSystemRolesQueryHandler(IIdentityService identity) : IRequestHandler<GetAllSystemRolesQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(GetAllSystemRolesQuery request, CancellationToken ct)
    {
        return await identity.GetAllRolesAsync(ct);
    }
}
