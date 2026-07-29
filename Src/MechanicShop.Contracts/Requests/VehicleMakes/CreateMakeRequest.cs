using FluentValidation;

public class CreateMakeRequest
{
    public string Make { get; set; } = string.Empty;
    public List<CreateModelRequest> Models { get; set; } = [];
}

public class CreateMakeRequestValidator : AbstractValidator<CreateMakeRequest>
{
    public CreateMakeRequestValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
            .WithMessage("Vehicle make is required.");

        RuleFor(x => x.Models)
            .NotEmpty()
            .WithMessage("At least one model is required.");

        RuleForEach(x => x.Models)
            .SetValidator(new CreateModelRequestValidator());
    }
}
