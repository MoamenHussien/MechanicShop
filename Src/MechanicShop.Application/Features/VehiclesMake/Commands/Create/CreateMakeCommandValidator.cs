using FluentValidation;

public class CreateMakeCommandValidator : AbstractValidator<CreateMakeCommand>
{
    public CreateMakeCommandValidator()
    {
        RuleFor(n=>n.Make).NotEmpty().Must(x=> !string.IsNullOrWhiteSpace(x)).WithMessage("You Must Enter Vehicle Make");
        RuleFor(n=>n.Models).NotNull().Must(n=>n.Count()>0).WithMessage("You Must Enter At Least One Model For Make");
        RuleForEach(n=>n.Models).SetValidator(new CreateVehicleModelCommandValidator());
    }
}
