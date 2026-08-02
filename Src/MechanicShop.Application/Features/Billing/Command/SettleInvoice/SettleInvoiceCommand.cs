using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record SettleInvoiceCommand(Guid InvoiceId) : IRequest<Result<Success>>;

public class SettleInvoiceCommandValidator : AbstractValidator<SettleInvoiceCommand>
{
    public SettleInvoiceCommandValidator()
    {
        RuleFor(n => n.InvoiceId).IdRequired("Invoice");
    }
}

public class SettleInvoiceCommandHandler(ILogger<SettleInvoiceCommandHandler> logger, TimeProvider time, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(SettleInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices.FindAsync([request.InvoiceId], cancellationToken);
        if (invoice is null)
        {
            logger.LogWarning("This Invoice Id : {InvoiceId} not found.", request.InvoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            logger.LogWarning("Invoice payment rejected. InvoiceId: {InvoiceId} has already been paid.", request.InvoiceId);
            return ApplicationErrors.InvoiceIsAlreadyPaid;
        }

        var paidStats = invoice.MarkAsPaid(time);

        if (paidStats.IsError)
        {
            logger.LogWarning("Invoice payment failed for InvoiceId: {InvoiceId} Errors: {@Errors}", invoice.Id, paidStats.Errors);
            return paidStats.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagAsync(CacheTags.Invoices, cancellationToken);

        logger.LogInformation("Invoice {InvoiceId} successfully paid.", invoice.Id);

        return Result.Success;
    }
}
