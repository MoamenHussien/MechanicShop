using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;

public sealed record UpdateLaborPermissionsCommand(Guid LaborId, List<string> Roles, List<Claim> Claims)
: IRequest<Result<Updated>>;

public class UpdateLaborPermissionsCommandValidator : AbstractValidator<UpdateLaborPermissionsCommand>
{
    public UpdateLaborPermissionsCommandValidator()
    {
        RuleFor(x => x.LaborId).IdRequired("Labor");

        RuleFor(x => x.Roles)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Roles are required.")
            .Must(n => n.Count > 0)
            .WithMessage("Must enter at least one role.");

        RuleForEach(x => x.Roles)
           .NotEmpty()
           .WithMessage("Role is required.")
           .IsEnumName(typeof(Role), caseSensitive: false)
           .WithMessage("Role must be a valid enum value");

        RuleFor(x => x.Claims).NotNull().WithMessage("The Claims Must Be Not Null");
    }
}

public class UpdateLaborPermissionsCommandHandler(ILogger<UpdateLaborPermissionsCommandHandler> logger, IIdentityService identity, ICacheInvalidator cacheInvalidator) :
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

        await cacheInvalidator.EvictByTagAsync(CacheTags.Users, cancellationToken);
        logger.LogInformation("Successfully updated permissions for labor {LaborId}.", request.LaborId);

        return Result.Updated;
    }
}