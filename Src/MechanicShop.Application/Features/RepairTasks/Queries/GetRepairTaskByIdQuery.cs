using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetRepairTaskByIdQuery(Guid id) : ICachedQuery<Result<RepairTaskDto>>
{
    public string CacheKey => $"RepairTask-{id}";

    public string[] Tags => ["RepairTasks"];

    public TimeSpan Expiration => TimeSpan.FromHours(24);
}

public class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(n=>n.id).IdRequired("Repair Tasks");
    }
}

public class GetRepairTaskByIdQueryHandler(ILogger<GetRepairTaskByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
    public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var RepairTask =await context.RepairTasks.AsNoTracking().Where(n=>n.Id==request.id).Include(n=>n.Parts).Select(n=>n.ToDto()).FirstOrDefaultAsync(cancellationToken);
        if (RepairTask is null)
        {
            logger.LogWarning("The Repair Task Is Not Found , For This Id : {id}",request.id);
            return ApplicationErrors.NotFoundThisRepairTaskId;
        }
            logger.LogInformation("Returning Repair Task Successfully To This Id : {id}",request.id);

        return RepairTask;
    }
}

