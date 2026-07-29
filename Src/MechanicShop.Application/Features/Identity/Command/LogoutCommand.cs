using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record LogoutCommand() : IRequest<Result<Success>>;

public sealed class LogoutCommandHandler(
    ILogger<LogoutCommandHandler> logger,
    IAppDbContext context,
    IIdentityService identity)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var refreshTokenResult = identity.GetRefreshTokenFromCookies();

        if (refreshTokenResult.IsSuccess)
        {
            var refreshToken = await context.RefreshTokens
                .SingleOrDefaultAsync(
                    x => x.Token == refreshTokenResult.Value,
                    cancellationToken);

            if (refreshToken is not null)
            {
                context.RefreshTokens.Remove(refreshToken);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "User {UserId} logged out successfully.",
                    refreshToken.UserId);
            }
            else
            {
                logger.LogDebug(
                    "Logout requested with an unknown refresh token.");
            }
        }

        identity.DeleteRefreshTokenCookie();

        return Result.Success;
    }
}
