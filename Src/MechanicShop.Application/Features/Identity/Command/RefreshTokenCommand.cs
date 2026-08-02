using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record RefreshTokenCommand(string ExpiredAccessToken) : IRequest<Result<TokenResponse>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(n => n.ExpiredAccessToken).NotEmpty().WithMessage("The Expired Access Token Is Required");
    }
}

public class RefreshTokenCommandHandler(ILogger<RefreshTokenCommandHandler> logger, IAppDbContext context, IIdentityService identity, ITokenProvider tokenProvider)
: IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var claimsPrincipal = tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (claimsPrincipal.IsError)
        {
            logger.LogWarning("The Expired Access Token Is Invalid : {token}", request.ExpiredAccessToken);
            return ApplicationErrors.InvalidAccessToken;
        }

        var userClaim = claimsPrincipal.Value.FindFirst(ClaimTypes.NameIdentifier);

        if (userClaim is null)
        {
            logger.LogWarning("The User Id Claim Is Missing");
            return ApplicationErrors.UserIdClaimInvalid;
        }

        var userIdResult = userClaim.Value.ToGuid();

        if (userIdResult.IsError)
        {
            logger.LogWarning("The User Id Claim Is Invalid");
            return ApplicationErrors.UserIdClaimInvalid;
        }

        Guid userId = userIdResult.Value;

        var refreshToken1 = identity.GetRefreshTokenFromCookies();

        if (refreshToken1.IsError)
        {
            logger.LogWarning("Failed to get refresh token from cookies. Error: {ErrorCode}", refreshToken1.TopError);
            return refreshToken1.Errors;
        }

        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(n => n.UserId == userId && n.Token == refreshToken1.Value);

        if (refreshToken is null || refreshToken.IsExpired())
        {
            logger.LogWarning("The Refresh Token Is Expired");
            return ApplicationErrors.RefreshTokenExpiredOrInvalid;
        }

        var userInfo = await identity.GetUserByIdAsync(userId);

        if (userInfo.IsError)
        {
            logger.LogWarning("Cant Get User Info From User Id : {id} , With This Errors : {@errors}", userId, userInfo.Errors);
            return userInfo.Errors;
        }

        var token = await tokenProvider.GenerateJwtTokenAsync(userInfo.Value);
        if (token.IsError)
        {
            logger.LogWarning("Is An Error During Generate JWT Token For This Id User : {id}", userId);
            return token.Errors;
        }

        logger.LogInformation("Generate Access Token For User Id : {id} ", userId);

        return token.Value;
    }
}
