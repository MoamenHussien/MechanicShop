using System.ComponentModel.Design.Serialization;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record CreateRepairTaskCommand(string name, decimal LaborCost, RepairDurationInMinutes duration, List<CreateRepairTaskPartCommand> Parts) : IRequest<Result<RepairTaskDto>>;

public class CreateRepairTaskCommandValidator : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(n => n.name).NotEmpty().WithMessage("Repair Task Name Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("You Must Enter Valid Repair Task Name").MaximumLength(50).WithMessage("The Maximum Length For Repair Task Name Is 50 Char");
        RuleFor(n => n.LaborCost).NotEmpty().WithMessage("Labor Costs Is Required").GreaterThan(0).WithMessage("The Labor Cost Must Be Greater Than 0");
        RuleFor(n => n.duration).IsInEnum().WithMessage("You Must Enter Valid Enum Duration");
        RuleFor(n => n.Parts).NotNull().WithMessage("Parts List Cannot Be Null").Must(n => n != null && n.Count > 0).WithMessage("At Least One Part Is Required");
        RuleForEach(n => n.Parts).SetValidator(new CreatePartCommandValidator());
    }
}

public class CreateRepairTaskCommandHandler(ILogger<CreateRepairTaskCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
    public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        string repairTaskName = request.name.CapitalizeFirstLetter();

        var isRepairTaskNameExists = await context.RepairTasks.AnyAsync(n => EF.Functions.Like(n.Name, repairTaskName), cancellationToken);
        if (isRepairTaskNameExists)
        {
            logger.LogWarning("The Repair Task Name Is Already Exists : {name}", repairTaskName);
            return RepairTaskErrors.DuplicateName;
        }

        List<Part> parts = new List<Part>();

        foreach (var item in request.Parts)
        {
            var cretePartStatus = Part.Create(Guid.NewGuid(), item.cost, item.name, item.Quantity);
            if (cretePartStatus.IsError)
            {
                logger.LogWarning("It Has An Error During Creating New Repair Task Part : {error}", cretePartStatus.Errors);
                return cretePartStatus.Errors;
            }

            parts.Add(cretePartStatus.Value);
        }

        var creteRepairTaskStatus = RepairTask.Create(Guid.NewGuid(), request.name, request.LaborCost, request.duration, parts);

        if (creteRepairTaskStatus.IsError)
        {
            logger.LogWarning("It Has An Error During Creating New Repair Task : {error}", creteRepairTaskStatus.Errors);
            return creteRepairTaskStatus.Errors;
        }

        await context.RepairTasks.AddAsync(creteRepairTaskStatus.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.RepairTasks, cancellationToken);

        logger.LogInformation("Created and saved new Repair Task successfully with Id: {Id} and removed the cache tag 'RepairTasks'", creteRepairTaskStatus.Value.Id);

        return creteRepairTaskStatus.Value.ToDto();
    }
}
