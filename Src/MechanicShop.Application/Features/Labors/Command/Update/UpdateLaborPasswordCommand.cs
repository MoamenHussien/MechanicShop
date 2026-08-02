using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record UpdateLaborPasswordCommand(string NewPassword, string CurrentPassword) : IRequest<Result<Success>>;

public class UpdateLaborPasswordCommandValidator : AbstractValidator<UpdateLaborPasswordCommand>
{
    public UpdateLaborPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password Is Required")
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Password")
            .Length(8, 30).WithMessage("Password must be between 8 and 30 characters");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current Password Is Required")
            .Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Current Password")
            .Length(8, 30).WithMessage("Current Password must be between 8 and 30 characters");
    }
}

internal sealed class UpdateLaborPasswordCommandHandler(IIdentityService identity, IUser user, ILogger<UpdateLaborPasswordCommandHandler> logger) : IRequestHandler<UpdateLaborPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateLaborPasswordCommand request, CancellationToken ct)
    {
        var userId = user.Id ?? Guid.Empty;
        var result = await identity.UpdateUserPasswordAsync(userId, request.NewPassword, request.CurrentPassword, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Password updated successfully for labor with ID {LaborId}.",
                userId);

            return Result.Success;
        }

        logger.LogWarning(
                "Failed to update password for labor with ID {LaborId}. Errors: {@Errors}",
                userId,
                result.Errors);

        return result.Errors;
    }
}
