using FluentValidation;

public class CreateModelRequest
{
    public string Model { get; set; } = string.Empty;
}

public class CreateModelRequestValidator : AbstractValidator<CreateModelRequest>
{
    public CreateModelRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Vehicle model is required.");
    }
}
