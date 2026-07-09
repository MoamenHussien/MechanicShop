using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetInvoiceByIdQuery(Guid invoiceId) : ICachedQuery<Result<InvoiceDto>>
{
    public string CacheKey => $"InvoiceId:{invoiceId}";

    public string[] Tags => ["Invoices"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(n=>n.invoiceId).IdRequired("Invoice");
    }
}

public class GetInvoiceByIdQueryHandler(ILogger<GetInvoiceByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var Invoice = await context.Invoices.AsNoTracking().Where(n=>n.Id==request.invoiceId)
                                      .Include(n=>n.InvoiceLineItems)
                                      .Include(n=>n.WorkOrder).ThenInclude(n=>n.Vehicle).ThenInclude(n=>n.Customer)
                                      .FirstOrDefaultAsync(cancellationToken);

        if (Invoice is null)
        {
            logger.LogWarning("This Invoice Is Not Found  ID : {id}", request.invoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        return Invoice.ToDto();
    }
}
