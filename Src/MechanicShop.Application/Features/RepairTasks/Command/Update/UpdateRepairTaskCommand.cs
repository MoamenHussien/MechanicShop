using System.Security.AccessControl;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record UpdateRepairTaskCommand(Guid id, string name, decimal LaborCost, RepairDurationInMinutes duration, List<UpdatePartCommand> Parts) : IRequest<Result<Updated>>;

public sealed class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
{
    public UpdateRepairTaskCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Repair Task");
        RuleFor(n => n.name).NotEmpty().WithMessage("Repair Task Name Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("You Must Enter Valid Repair Task Name").MaximumLength(50).WithMessage("The Maximum Length For Repair Task Name Is 50 Char");
        RuleFor(n => n.LaborCost).NotEmpty().WithMessage("Labor Costs Is Required").GreaterThan(0).WithMessage("The Labor Cost Must Be Greater Than 0");
        RuleFor(n => n.duration).IsInEnum().WithMessage("You Must Enter Valid Enum Duration");
        RuleFor(n => n.Parts).NotNull().WithMessage("Repair Task Parts Is Required").Must(n => n != null && n.Count > 0).WithMessage("At least one part is required");
        RuleForEach(n => n.Parts).SetValidator(new UpdatePartCommandValidator());
    }
}

public class UpdateRepairTaskCommandHandler(ILogger<UpdateRepairTaskCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var repairTask = await context.RepairTasks.Where(n => n.Id == request.id).Include(n => n.Parts).FirstOrDefaultAsync(cancellationToken);

        if (repairTask is null)
        {
            logger.LogWarning("The Repair Task Is Not Found , For This Id : {id}", request.id);
            return ApplicationErrors.NotFoundThisRepairTaskId;
        }

        var repairTaskName = request.name.CapitalizeFirstLetter();
        var nameIsExists = await context.RepairTasks.AnyAsync(n => n.Id != request.id && EF.Functions.Like(n.Name, repairTaskName));

        if (nameIsExists)
        {
            logger.LogWarning("The Repair Task Name Is Already Exists : {name}", repairTaskName);
            return RepairTaskErrors.DuplicateName;
        }

        List<Part> parts = new List<Part>();

        foreach (var part in request.Parts)
        {
            var partId = part.id ?? Guid.NewGuid();
            var cretePartStatus = Part.Create(partId, part.cost, part.name, part.Quantity);

            if (cretePartStatus.IsError)
            {
                logger.LogWarning("It Has An Error During Creating New Repair Task Part : {@error}", cretePartStatus.Errors);
                return cretePartStatus.Errors;
            }

            parts.Add(cretePartStatus.Value);
        }

        var updateRepairTaskStatus = repairTask.Update(request.name, request.LaborCost, request.duration);

        if (updateRepairTaskStatus.IsError)
        {
            logger.LogWarning("It Has An Error During Updating The Repair Task Info : {@error}", updateRepairTaskStatus.Errors);
            return updateRepairTaskStatus.Errors;
        }

        var upsertRepairTaskStatus = repairTask.UpSert(parts);

        if (upsertRepairTaskStatus.IsError)
        {
            logger.LogWarning("It Has An Error During UpSert The Repair Task Parts : {@error}", upsertRepairTaskStatus.Errors);
            return upsertRepairTaskStatus.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.RepairTasks, cancellationToken);

        logger.LogInformation("Updated Info and UpSered Part For Repair Task Is successfully with Id: {Id} , And removed the cache tag 'RepairTasks'", request.id);

        return Result.Updated;
    }
}
