namespace MechanicShop.Tests.Common.Auth;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> CreateRefreshToken(Guid? id = null, string? token = null, Guid? userId = null, DateTimeOffset? expiresOnUtc = null)
    {
        return RefreshToken.Create(
            id ?? Guid.NewGuid(),
            token ?? "sometoken",
            userId ?? Guid.NewGuid(),
            expiresOnUtc ?? DateTime.UtcNow.AddDays(7));
    }
}
