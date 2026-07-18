using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetRepairTasksQuery : ICachedQuery<Result<List<RepairTaskDto>>>
{
    public string CacheKey => "RepairTasks";

    public string[] Tags => ["RepairTasks"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(24);
}

public class GetRepairTasksQueryHandler(ILogger<GetRepairTasksQuery> logger, IAppDbContext context)
: IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
    public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
    {
        var RepairTasks = await context.RepairTasks.AsNoTracking().Select(r => new RepairTaskDto
        {
            RepairTaskId = r.Id,
            Name = r.Name,
            EstimatedDurationInMins = r.EstimatedDuration,
            LaborCost = r.LaborCost,
            Parts = r.Parts.Select(p => new PartDto(p.Id,p.Name,p.Costs,p.Quantity)).ToList()

        }).ToListAsync(cancellationToken);

        if (!RepairTasks.Any())
        {
            logger.LogWarning("Not Found Any Repair Tasks");
            return ApplicationErrors.NotFoundAnyRepairTasks;
        }

        logger.LogInformation("Returning All Repair Tasks And Count Is : {count}", RepairTasks.Count);

        return RepairTasks;
    }
}