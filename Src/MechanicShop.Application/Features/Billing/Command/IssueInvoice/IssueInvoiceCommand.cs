using System.Reflection.Metadata;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record IssueInvoiceCommand(Guid workOrderId) : IRequest<Result<InvoiceDto>>;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(n => n.workOrderId).IdRequired("Work Order");
    }
}

public sealed class IssueInvoiceCommandHandler(ILogger<IssueInvoiceCommand> logger, IMediator mediator, IAppDbContext context, ICacheInvalidator cacheInvalidator, TimeProvider time)
: IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        // if (!await identity.IsInRoleAsync(user.Id!.Value, Role.Manager.ToString()))
        // {
        //     logger.LogWarning("Issue Invoice : (Forbidden) , This User {UserId} is not allowed to Issue The Invoice", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }
        if (await context.Invoices.AnyAsync(n => n.WorkOrderId == request.workOrderId, cancellationToken))
        {
            logger.LogWarning("Issue Invoice Cancelled: Invoice has already been issued for WorkOrder Id '{WorkOrderId}'.", request.workOrderId);
            return ApplicationErrors.InvoiceAlreadyIssued;
        }

        var workOrder = await context.WorkOrders.AsNoTracking()
                                          .Include(n => n.RepairTasks).ThenInclude(n => n.Parts).
                                           FirstOrDefaultAsync(n => n.Id == request.workOrderId, cancellationToken);

        if (workOrder is null)
        {
            logger.LogWarning("This Work Order Is Not Found  ID : {id}", request.workOrderId);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        if (workOrder.State is not WorkOrderState.Completed)
        {
            logger.LogWarning("Issue Invoice Denied: WorkOrder '{WorkOrderId}' is in '{State}' state. Invoice can only be issued for Completed work orders.", request.workOrderId, workOrder.State);
            return ApplicationErrors.WorkOrderMustBeCompletedToIssueInvoice;
        }

        var invoiceLineItems = new List<InvoiceLineItem>();
        var invoiceID = Guid.NewGuid();

        foreach (var (repairTask, repairTaskIndex) in workOrder.RepairTasks.Select((R, I) => (R, I + 1)))
        {
            var partDescription = string.Empty;
            var lineNumber = repairTaskIndex;
            if (repairTask.Parts.Any())
            {
                partDescription = string.Join(Environment.NewLine, repairTask.Parts.Select(n => $"    • {n.Name} | Qty: {n.Quantity} × {n.Costs:C} = { n.Quantity * n.Costs:C}"));
            }
            else
            {
                partDescription = "    • No Parts";
            }

            var description = $"{repairTaskIndex} : {repairTask.Name}{Environment.NewLine}" +
                              $"  Labor = {repairTask.LaborCost:c}{Environment.NewLine}" +
                              $"  Parts:{Environment.NewLine}" + partDescription;
            var quantity = 1;
            var unitPrice = repairTask.LaborCost + repairTask.Parts.Sum(n => n.Costs * n.Quantity);

            var invoiceLineItem = InvoiceLineItem.Create(invoiceID, description, lineNumber, unitPrice, quantity);
            if (invoiceLineItem.IsError)
            {
                return invoiceLineItem.Errors;
            }

            invoiceLineItems.Add(invoiceLineItem.Value);
        }

        var subTotal = invoiceLineItems.Sum(n => n.LineTotal);

        var taxAmount = subTotal * MechanicShopConstants.TaxRate;
        var discountAmount = workOrder.Discount ?? 0m;

        var invoice = Invoice.Create(invoiceID, time, taxAmount, discountAmount, invoiceLineItems, request.workOrderId);
        if (invoice.IsError)
        {
            return invoice.Errors;
        }

        await context.Invoices.AddAsync(invoice.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagsAsync(cancellationToken, CacheTags.Invoices, CacheTags.WorkOrders);

        logger.LogInformation("Invoice issued successfully for WorkOrder {WorkOrderId}. Cache 'Invoices' was invalidated. InvoiceId: {InvoiceId}", request.workOrderId, invoiceID);

        return await mediator.Send(new GetInvoiceByIdQuery(invoice.Value.Id), cancellationToken);
    }
}
