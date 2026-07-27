using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;

public sealed record ReAssignLaborCommand(Guid WorkOrderId, Guid LaborId) : IRequest<Result<Updated>>;

public class ReAssignLaborCommandValidator : AbstractValidator<ReAssignLaborCommand>
{
    public ReAssignLaborCommandValidator()
    {
        RuleFor(n => n.WorkOrderId).IdRequired("WorkOrder");
        RuleFor(n => n.LaborId).IdRequired("Labor");
    }
}



public class ReAssignLaborCommandHandler(ILogger<ReAssignLaborCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator,IUser user ,IWorkOrderPolicy policy)
: IRequestHandler<ReAssignLaborCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ReAssignLaborCommand request, CancellationToken cancellationToken)
    {
        // if(await identity.IsInRoleAsync(user.Id!.Value,Role.Labor.ToString()))
        // {
        //     logger.LogWarning("ReAssignLabor: (Forbidden) , This User {UserId} is not allowed to reassign labor", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }

        var WordOrder = await context.WorkOrders.FindAsync([request.WorkOrderId],cancellationToken);

        if (WordOrder is null)
        {
            logger.LogWarning("ReAssignLabor: WorkOrder not found. WorkOrderId={WorkOrderId}", request.WorkOrderId);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        if (WordOrder.LaborId == request.LaborId)
        {
            return ApplicationErrors.NothingIsChanged;
        }

        var laborExits = await context.Employees.FindAsync([request.LaborId],cancellationToken);

        if (laborExits is null)
        {
            logger.LogWarning("ReAssignLabor: Labor not found. LaborId={LaborId}", request.LaborId);
            return ApplicationErrors.NotFoundTheLabor;
        }

        if (await policy.IsLaborOccupiedDuringRange(WordOrder.StartAtUtc, WordOrder.EndAtUtc, request.LaborId,request.WorkOrderId,cancellationToken))
        {
            logger.LogWarning("ReAssignLabor: Labor {LaborId} is already occupied during the work order time range.", request.LaborId);
            return ApplicationErrors.ThisLaborHasAnotherWorkOrderAtThisRangeTime;
        }
    
        var ReAssignState = WordOrder.ReAssignLabor(request.LaborId);
        if (ReAssignState.IsError)
        {
            logger.LogWarning("ReAssignLabor: WorkOrder {WorkOrderId} is in state {State}, cannot reassign labor",request.WorkOrderId, WordOrder.State);
            return ReAssignState.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.WorkOrders, cancellationToken);

        logger.LogInformation("ReAssignLabor: Successfully reassigned Labor {LaborId} to WorkOrder {WorkOrderId} by User {UserId}", request.LaborId, request.WorkOrderId, user.Id);

        return Result.Updated;
    }
}