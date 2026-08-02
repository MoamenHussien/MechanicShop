using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record CreateRepairTaskPartCommand(string name, decimal cost, int Quantity) : IRequest<Result<Success>>;

public class CreatePartCommandValidator : AbstractValidator<CreateRepairTaskPartCommand>
{
    public CreatePartCommandValidator()
    {
        RuleFor(n => n.name).NotEmpty().WithMessage("Part Name Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("You Must Enter Valid Part Name").MaximumLength(50).WithMessage("The Maximum Length For Part Name Is 50 Char");
        RuleFor(n => n.cost).NotEmpty().WithMessage("Part Costs Is Required").GreaterThan(0).WithMessage("The Part Cost Must Be Greater Than 0");
        RuleFor(n => n.Quantity).NotEmpty().WithMessage("Part Quantity Is Required").GreaterThan(0).WithMessage("The Part Quantity Must Be Greater Than 0)");
    }
}
