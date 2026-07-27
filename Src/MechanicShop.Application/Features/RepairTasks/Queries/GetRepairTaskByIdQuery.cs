using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MechanicShop.Application.Common.Constants;

public sealed record GetRepairTaskByIdQuery(Guid id) : ICachedQuery<Result<RepairTaskDto>>
{
    public string CacheKey => $"RepairTask-{id}";

    public string[] Tags => [CacheTags.RepairTasks];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(n => n.id).IdRequired("Repair Tasks");
    }
}

public class GetRepairTaskByIdQueryHandler(ILogger<GetRepairTaskByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
    public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var repairTask = await context.RepairTasks.AsNoTracking()
                                                                .Where(r => r.Id == request.id)
                                                                .Select(r => new RepairTaskDto
                                                                {
                                                                    RepairTaskId = r.Id,
                                                                    Name = r.Name,
                                                                    EstimatedDurationInMins = r.EstimatedDuration,
                                                                    LaborCost = r.LaborCost,
                                                                    Parts = r.Parts
                                                                        .Select(p => new PartDto(
                                                                            p.Id,
                                                                            p.Name,
                                                                            p.Costs,
                                                                            p.Quantity))
                                                                        .ToList()
                                                                })
                                                                .FirstOrDefaultAsync(cancellationToken);

        if (repairTask is null)
        {
            logger.LogWarning("The Repair Task Is Not Found , For This Id : {id}", request.id);
            return ApplicationErrors.NotFoundThisRepairTaskId;
        }
        logger.LogInformation("Returning Repair Task Successfully To This Id : {id}", request.id);

        return repairTask;
    }
}

