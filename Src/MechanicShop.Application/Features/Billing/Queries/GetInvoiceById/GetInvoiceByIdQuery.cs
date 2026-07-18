using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        RuleFor(n => n.invoiceId).IdRequired("Invoice");
    }
}

public class GetInvoiceByIdQueryHandler(ILogger<GetInvoiceByIdQueryHandler> logger, IAppDbContext context)
: IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices.Select(InvoiceMapper.InvoiceProjection)
                                            .FirstOrDefaultAsync(n=>n.InvoiceId == request.invoiceId, cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("This Invoice Is Not Found  ID : {id}", request.invoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        return invoice;
    }
}
