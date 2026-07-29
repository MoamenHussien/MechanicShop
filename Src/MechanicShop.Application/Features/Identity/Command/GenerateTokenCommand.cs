using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record GenerateTokenCommand(string email, string password) : IRequest<Result<TokenResponse>>;

public class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
{
    public GenerateTokenCommandValidator()
    {
        RuleFor(n => n.email).MustBeValidEmail();
        RuleFor(n => n.password).NotEmpty().WithMessage("Password Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Password").Length(8, 30).WithMessage("Password must be between 8 and 30 characters");
    }
}

public class GenerateTokenCommandHandler(ILogger<GenerateTokenCommandHandler> logger, ITokenProvider token, IIdentityService identity)
: IRequestHandler<GenerateTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
    {
        var UserDto = await identity.AuthenticateAsync(request.email, request.password, cancellationToken);

        if (UserDto.IsError)
        {
            logger.LogWarning("Login failed for email: {Email} - {@ErrorDescription}", request.email, UserDto.Errors);
            return UserDto.Errors;
        }

        var TokenResponse = await token.GenerateJwtTokenAsync(UserDto.Value);

        if (TokenResponse.IsError)
        {
            logger.LogError("Token generation failed for user: {UserId} - {ErrorDescription}", UserDto.Value.UserId, TokenResponse.Errors);
            return TokenResponse.Errors;
        }

        logger.LogInformation("The Authentication And Generate Jwt Token To This User Id : {id} Is Successfully", UserDto.Value.UserId);
        return TokenResponse.Value;
    }
}
