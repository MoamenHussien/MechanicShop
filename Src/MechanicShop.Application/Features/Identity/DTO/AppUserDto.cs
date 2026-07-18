using System.Security.Claims;

public sealed record AppUserDto(Guid UserId, string Email, IList<string> Roles, IList<Claim> Claims);