using System.Security.Claims;
using FluentValidation;

namespace MechanicShop.Contracts.Requests.Labors;

public class UpdateLaborPermissionsRequest
{
    public List<string> Roles { get; set; } = null!;
    public List<Claim>? Claims { get; set; }
}

public class UpdateLaborPermissionsCommandValidatorContract : AbstractValidator<UpdateLaborPermissionsRequest>
{
    public UpdateLaborPermissionsCommandValidatorContract()
    {
        RuleForEach(x => x.Roles)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role != "Manager").WithMessage("Manager role cannot be assigned.");
    }
}
