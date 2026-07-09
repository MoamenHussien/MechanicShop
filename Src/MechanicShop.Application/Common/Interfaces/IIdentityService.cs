using System.Security.Claims;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(Guid userId, string role);
    Task<Result<HashSet<Guid>>> GetIdsOfUsersByRoleTypeAsync(Role role);
    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password,CancellationToken ct);
    Task<Result<AppUserDto>> GetUserByIdAsync(Guid id);
    Task<string?> GetUserNameAsync(Guid Userid);
    Task<Result<Guid>> CreateUserAsync(string email, string password, IList<string> roles, IList<Claim> claims,CancellationToken ct);
    Task<Result<bool>> UpdateUserPermissionsAsync(Guid userid, IList<string> roles, IList<Claim> claims,CancellationToken ct);
    Task<Result<Success>> DeleteUserAsync(Guid userid);
    Result<string> GetRefreshTokenFromCookies();
}