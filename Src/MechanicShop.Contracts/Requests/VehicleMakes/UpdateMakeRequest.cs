using FluentValidation;

public class UpdateMakeRequest
{
    public string Make { get; set; } = string.Empty;
    public List<UpdateModelRequest> Models { get; set; } = [];
}

public class UpdateMakeRequestValidator : AbstractValidator<UpdateMakeRequest>
{
    public UpdateMakeRequestValidator()
    {
        RuleFor(x => x.Make).NotEmpty().WithMessage("Vehicle make is required.");

        RuleFor(x => x.Models).NotEmpty().WithMessage("At least one vehicle model is required.");

        RuleForEach(x => x.Models).SetValidator(new UpdateModelRequestValidator());
    }
}