using System.Security.Claims;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

    Result<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
}
