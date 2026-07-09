using System.Security.Claims;

public sealed record AppUserDto(Guid UserId,string email,IList<string>Roles,IList<Claim>Claims);