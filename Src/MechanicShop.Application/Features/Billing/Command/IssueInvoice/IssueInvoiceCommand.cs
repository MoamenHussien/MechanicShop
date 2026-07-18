using System.Reflection.Metadata;
using FluentValidation;
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

public sealed class IssueInvoiceCommandHandler(ILogger<IssueInvoiceCommand> logger,IMediator mediator , IAppDbContext context, HybridCache cache,TimeProvider time)
: IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        // if (!await identity.IsInRoleAsync(user.Id!.Value, Role.Manager.ToString()))
        // {
        //     logger.LogWarning("Issue Invoice : (Forbidden) , This User {UserId} is not allowed to Issue The Invoice", user.Id);
        //     return ApplicationErrors.NotAllowed;
        // }

        if (await context.Invoices.AnyAsync(n => n.WorkOrderId == request.workOrderId,cancellationToken))
        {
            logger.LogWarning("Issue Invoice Cancelled: Invoice has already been issued for WorkOrder Id '{WorkOrderId}'.", request.workOrderId);
            return ApplicationErrors.InvoiceAlreadyIssued;
        }

        var WorkOrder = await context.WorkOrders.AsNoTracking()
                                          .Include(n => n.RepairTasks).ThenInclude(n => n.Parts).
                                           FirstOrDefaultAsync(n => n.Id == request.workOrderId,cancellationToken);

        if (WorkOrder is null)
        {
            logger.LogWarning("This Work Order Is Not Found  ID : {id}", request.workOrderId);
            return ApplicationErrors.NotFoundTheWorkOrder;
        }

        if (WorkOrder.State is not WorkOrderState.Completed)
        {
            logger.LogWarning("Issue Invoice Denied: WorkOrder '{WorkOrderId}' is in '{State}' state. Invoice can only be issued for Completed work orders.", request.workOrderId, WorkOrder.State);
            return ApplicationErrors.WorkOrderMustBeCompletedToIssueInvoice;
        }



        var InvoiceLineItems = new List<InvoiceLineItem>();
        var invoiceID = Guid.NewGuid();

        foreach (var (RepairTask, RepairTaskIndex) in WorkOrder.RepairTasks.Select((R, I) => (R, I + 1)))
        {
            var PartDescription = string.Empty;
            var LineNumber = RepairTaskIndex;
            if (RepairTask.Parts.Any())
            {
                PartDescription = string.Join(Environment.NewLine, RepairTask.Parts.Select(n => $"    • {n.Name} | Qty: {n.Quantity} × {n.Costs:C} = {(n.Quantity * n.Costs):C}"));
            }
            else
            {
                PartDescription = "    • No Parts";
            }

            var Description = $"{RepairTaskIndex} : {RepairTask.Name}{Environment.NewLine}" +
                              $"  Labor = {RepairTask.LaborCost:c}{Environment.NewLine}" +
                              $"  Parts:{Environment.NewLine}" + PartDescription;
            var Quantity = 1;
            var UnitPrice = RepairTask.LaborCost + RepairTask.Parts.Sum(n => n.Costs * n.Quantity);

            var invoiceLineItem = InvoiceLineItem.Create(invoiceID, Description, LineNumber, UnitPrice, Quantity);
            if (invoiceLineItem.IsError)
            {
                return invoiceLineItem.Errors;
            }

            InvoiceLineItems.Add(invoiceLineItem.Value);

        }
        var SubTotal = InvoiceLineItems.Sum(n=>n.LineTotal);

        var TaxAmount = SubTotal * MechanicShopConstants.TaxRate;
        var DiscountAmount = WorkOrder.Discount ?? 0m;

        var invoice = Invoice.Create(invoiceID, time, TaxAmount, DiscountAmount, InvoiceLineItems, request.workOrderId);
        if (invoice.IsError)
        {
            return invoice.Errors;
        }

        await context.Invoices.AddAsync(invoice.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("Invoices", cancellationToken);

        logger.LogInformation("Invoice issued successfully for WorkOrder {WorkOrderId}. Cache 'Invoices' was invalidated. InvoiceId: {InvoiceId}",request.workOrderId,invoiceID);

        return await mediator.Send(new GetInvoiceByIdQuery(invoice.Value.Id),cancellationToken);

    }
}