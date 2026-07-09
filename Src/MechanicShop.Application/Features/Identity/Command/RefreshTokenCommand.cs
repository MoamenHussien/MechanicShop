using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record RefreshTokenCommand(string AccessToken) : IRequest<Result<TokenResponse>>;
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(n => n.AccessToken).NotEmpty().WithMessage("The Access Token Is Required");
    }
}

public class RefreshTokenCommandHandler(ILogger<RefreshTokenCommandHandler> logger, IAppDbContext context, IIdentityService identity, ITokenProvider token)
: IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var ClaimsPrincipal = token.GetPrincipalFromExpiredToken(request.AccessToken);

        if (ClaimsPrincipal is null)
        {
            logger.LogWarning("The Expired Access Token Is Invalid : {token}", request.AccessToken);
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userClaim = ClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

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

        Guid UserId = userIdResult.Value;

        var RefreshToken = identity.GetRefreshTokenFromCookies();

        if (RefreshToken.IsError)
        {
            logger.LogWarning("The Refresh Token Is Expired");
            return ApplicationErrors.RefreshTokenExpired;
        }

        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(n => n.UserId == UserId && n.Token == RefreshToken.Value);

        if (refreshToken is null || refreshToken.IsExpired())
        {
            logger.LogWarning("The Refresh Token Is Expired");
            return ApplicationErrors.RefreshTokenExpired;
        }

        var UserInfo = await identity.GetUserByIdAsync(UserId);

        if (UserInfo.IsError)
        {
            logger.LogWarning("Cant Get User Info From User Id : {id} , With This Errors : {@errors}", UserId, UserInfo.Errors);
            return UserInfo.Errors;
        }

        var Token = await token.GenerateJwtTokenAsync(UserInfo.Value);
        if (Token.IsError)
        {
            logger.LogWarning("Is An Error During Generate JWT Token For This Id User : {id}", UserId);
            return Token.Errors;
        }

        logger.LogInformation("Generate Access Token For User Id : {id} ", UserId);

        return Token.Value;
    }
}
