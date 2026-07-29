using FluentValidation;

public class UpdateModelRequest
{
    public Guid? ModelId { get; set; }

    public string Model { get; set; } = string.Empty;
}

public class UpdateModelRequestValidator : AbstractValidator<UpdateModelRequest>
{
    public UpdateModelRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Vehicle model name is required.");
    }
}
