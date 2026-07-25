using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record UpdateLaborPermissionsCommand(Guid LaborId, List<string> Roles, List<Claim> Claims)
: IRequest<Result<Updated>>;

public class UpdateLaborPermissionsCommandValidator : AbstractValidator<UpdateLaborPermissionsCommand>
{
    public UpdateLaborPermissionsCommandValidator()
    {
        RuleFor(x => x.LaborId).IdRequired("Labor");
        RuleForEach(x => x.Roles).NotEmpty().WithMessage("Role is required.");
        RuleForEach(x => x.Roles).IsEnumName(typeof(Role), caseSensitive: false).WithMessage("Role must be a valid enum value");
        RuleFor(x => x.Claims).NotNull().WithMessage("The Claims Must Be Not Null");
    }
}
public class UpdateLaborPermissionsCommandHandler(ILogger<UpdateLaborPermissionsCommandHandler> logger, IIdentityService identity, HybridCache cache) :
IRequestHandler<UpdateLaborPermissionsCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateLaborPermissionsCommand request, CancellationToken cancellationToken)
    {
        var result = await identity.UpdateUserPermissionsAsync(request.LaborId, request.Roles, request.Claims, cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update permissions for labor {LaborId}. Errors: {@Errors}", request.LaborId, result.Errors);

            return result.Errors;
        }

        await cache.RemoveByTagAsync("Labors", cancellationToken);
        await cache.RemoveByTagAsync("Employees", cancellationToken);
        logger.LogInformation("Successfully updated permissions for labor {LaborId}.", request.LaborId);

        return Result.Updated;
    }
}