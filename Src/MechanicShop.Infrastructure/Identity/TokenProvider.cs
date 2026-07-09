using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MechanicShop.Infrastructure.Identity;

public class TokenProvider(IOptions<JwtSettings> JwtConfigSettings, IAppDbContext context, IHttpContextAccessor httpContextAccessor)
    : ITokenProvider
{
    private readonly IAppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
    {
        var tokenResult = await CreateAsync(user, ct);

        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfigSettings.Value.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = JwtConfigSettings.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtConfigSettings.Value.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }

    private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken ct = default)
    {
        var issuer = JwtConfigSettings.Value.Issuer;
        var audience = JwtConfigSettings.Value.Audience;
        var key = JwtConfigSettings.Value.SecretKey;
        var expires = DateTime.UtcNow.AddMinutes(JwtConfigSettings.Value.TokenExpirationInMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new (JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new (JwtRegisteredClaimNames.Email, user.email!),
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        foreach (var claim in user.Claims)
        {
            claims.Add(claim);
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);

        var oldRefreshTokens = await _context.RefreshTokens
              .Where(rt => rt.UserId == user.UserId)
              .ExecuteDeleteAsync(ct);

        var refreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTime.UtcNow.AddDays(7));

        if (refreshTokenResult.IsError)
        {
            return refreshTokenResult.Errors;
        }

        var refreshToken = refreshTokenResult.Value;
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            httpContext.Response.Cookies.Append("RefreshToken", refreshToken.Token, cookieOptions);
        }

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = null,
            ExpiresOnUtc = expires
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}