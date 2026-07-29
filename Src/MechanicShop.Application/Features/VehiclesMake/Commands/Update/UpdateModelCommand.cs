using System.Data;
using FluentValidation;
using MediatR;

public sealed record UpdateModelCommand(Guid? ModelId, string model) : IRequest<Result<Updated>>;

public class UpdateModelCommandValidator : AbstractValidator<UpdateModelCommand>
{
    public UpdateModelCommandValidator()
    {
        RuleFor(n => n.model).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x))
        .WithMessage("You Must Enter Vehicle Model Name");
    }
}

