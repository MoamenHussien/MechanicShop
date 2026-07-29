using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;

public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result<Deleted>>;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(n => n.CustomerId).IdRequired("Customer");
    }
}



public class DeleteCustomerCommandHandler(ILogger<DeleteCustomerCommandHandler> logger, ICacheInvalidator cacheInvalidator, IAppDbContext context, IWorkOrderPolicy policy)
: IRequestHandler<DeleteCustomerCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var Customer = await context.Customers.FindAsync(request.CustomerId, cancellationToken);

        if (Customer is null)
        {
            logger.LogWarning("The Customer Is Not Found With Id : {id}", request.CustomerId);
            return ApplicationErrors.TheCustomerNotFound;
        }

        if (await policy.IsThisCustomerHasAnyRequestForWorkOrderBeforeAsync(request.CustomerId, cancellationToken))
        {
            logger.LogWarning("This Customer With Id : {id} Has Record At Work Order Table", request.CustomerId);
            return ApplicationErrors.TheCustomerHasRecordForWorkOrderBefore;
        }
        context.Customers.Remove(Customer);
        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.Customers, cancellationToken);

        logger.LogInformation("Successfully Deleted The Customer With Id : {id} And Removed Cache Tag With Name Customer ", request.CustomerId);

        return Result.Deleted;
    }
}
