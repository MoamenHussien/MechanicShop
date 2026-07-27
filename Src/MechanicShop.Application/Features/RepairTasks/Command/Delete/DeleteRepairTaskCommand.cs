using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;

public sealed record DeleteRepairTaskCommand(Guid id) : IRequest<Result<Deleted>>;

public class DeleteRepairTaskCommandValidator : AbstractValidator<DeleteRepairTaskCommand>
{
    public DeleteRepairTaskCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Repair Task");
    }
}



public class DeleteRepairTaskCommandHandler(ILogger<DeleteCustomerCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<DeleteRepairTaskCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteRepairTaskCommand request, CancellationToken cancellationToken)
    {
        var RepairTask = await context.RepairTasks.FindAsync(request.id,cancellationToken);
        if (RepairTask is null)
        {
            logger.LogWarning("The Repair Task Is Not Found , For This Id : {id}", request.id);
            return ApplicationErrors.NotFoundThisRepairTaskId;
        }

        var IsUsed = await context.WorkOrders.AsNoTracking().AnyAsync(n => n.RepairTasks.Any(n => n.Id == request.id), cancellationToken);

        if (IsUsed)
        {
            logger.LogWarning("The Repair Task Cant Deleted Because Is Used At Work Orders : {id}",request.id);
            return RepairTaskErrors.InUse;
        }

        context.RepairTasks.Remove(RepairTask);
        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.RepairTasks, cancellationToken);

        logger.LogInformation("Deleted the Repair Task successfully with Id: {Id} and removed the cache tag 'RepairTasks' ", request.id);

        return Result.Deleted;

    }
}