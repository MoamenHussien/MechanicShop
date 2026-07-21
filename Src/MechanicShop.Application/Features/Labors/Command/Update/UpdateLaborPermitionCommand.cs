using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record UpdateLaborPermissionsCommand(Guid LaborId, List<string> Roles, List<Claim> Claims)
: IRequest<Result<Updated>>;

public class UpdateLaborPermissionsCommandValidator : AbstractValidator<UpdateLaborPermissionsCommand>
{
    public UpdateLaborPermissionsCommandValidator()
    {
        RuleFor(x => x.LaborId).IdRequired("Labor");
        RuleFor(x => x.Roles).NotEmpty().WithMessage("At least one role is required");
        RuleForEach(x => x.Roles).IsEnumName(typeof(Role), caseSensitive: false).WithMessage("Role must be a valid enum value");
        RuleFor(x => x.Claims).NotNull().WithMessage("The Claims Must Be Not Null");
    }
}
public class UpdateLaborPermissionsCommandHandler(ILogger<UpdateLaborPermissionsCommandHandler> logger, IIdentityService identity) :
IRequestHandler<UpdateLaborPermissionsCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateLaborPermissionsCommand request, CancellationToken cancellationToken)
    {
        var result = await identity.UpdateUserPermissionsAsync(request.LaborId, request.Roles, request.Claims,cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Failed to update permissions for labor {LaborId}. Errors: {@Errors}",request.LaborId,result.Errors);

            return result.Errors;
        }

        logger.LogInformation("Successfully updated permissions for labor {LaborId}.", request.LaborId);

        return Result.Updated;
    }
}