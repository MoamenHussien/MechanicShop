using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record ResetLaborPasswordCommand(Guid LaborId) : IRequest<Result<Success>>;

public class ResetLaborPasswordCommandValidator : AbstractValidator<ResetLaborPasswordCommand>
{
    public ResetLaborPasswordCommandValidator()
    {
        RuleFor(x => x.LaborId).IdRequired("Labor");
    }
}

internal sealed class ResetLaborPasswordCommandHandler(IIdentityService identity, ILogger<ResetLaborPasswordCommandHandler> logger) : IRequestHandler<ResetLaborPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResetLaborPasswordCommand request, CancellationToken ct)
    {
        var result = await identity.ResetUserPasswordAsync(request.LaborId);
        if (result.IsSuccess)
        {
            logger.LogInformation("Password reset completed For Labor {LaborId}", request.LaborId);
            return Result.Success;
        }

        logger.LogError("Password reset failed for Labor {LaborId}", request.LaborId);
        return result.Errors;
    }
}
