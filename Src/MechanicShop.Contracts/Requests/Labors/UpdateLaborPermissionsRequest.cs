using System.Security.Claims;
using FluentValidation;

public class UpdateLaborPermissionsRequest
{
    public List<string> Roles { get;  set; } =null!;
    public List<Claim> Claims { get;  set; } =null!;
}

public class UpdateLaborPermissionsCommandValidatorContract : AbstractValidator<UpdateLaborPermissionsRequest>
{
    public UpdateLaborPermissionsCommandValidatorContract()
    {
        RuleForEach(n=>n.Roles).Must(n=> n.Count() > 0).WithMessage("At least one role is required");
        RuleForEach(x => x.Roles).IsInEnum().WithMessage("Role must be a valid enum value");
        RuleFor(x => x.Claims).NotNull().WithMessage("The Claims Must Be Not Null");
    }
}