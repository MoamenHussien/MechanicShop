using FluentValidation;

public class CreateVehicleModelCommandValidator : AbstractValidator<CreateVehicleModelCommand>
{
    public CreateVehicleModelCommandValidator()
    {
        RuleFor(n=>n.model).NotEmpty().Must(x=> !string.IsNullOrWhiteSpace(x)).
        WithMessage("You Must Enter Vehicle Model Name");
    }
}