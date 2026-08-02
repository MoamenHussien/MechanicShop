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
        var userDto = await identity.AuthenticateAsync(request.email, request.password, cancellationToken);

        if (userDto.IsError)
        {
            logger.LogWarning("Login failed for email: {Email} - {@ErrorDescription}", request.email, userDto.Errors);
            return userDto.Errors;
        }

        var tokenResponse = await token.GenerateJwtTokenAsync(userDto.Value);

        if (tokenResponse.IsError)
        {
            logger.LogError("Token generation failed for user: {UserId} - {ErrorDescription}", userDto.Value.UserId, tokenResponse.Errors);
            return tokenResponse.Errors;
        }

        logger.LogInformation("The Authentication And Generate Jwt Token To This User Id : {id} Is Successfully", userDto.Value.UserId);
        return tokenResponse.Value;
    }
}
