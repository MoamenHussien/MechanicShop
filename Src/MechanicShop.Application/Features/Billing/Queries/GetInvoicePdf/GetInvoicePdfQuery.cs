using System.Numerics;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record GetInvoicePdfQuery(Guid InvoiceId) : IRequest<Result<InvoicePdfDto>>;

public sealed class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(n => n.InvoiceId).IdRequired("Invoice");
    }
}

public class GetInvoicePdfQueryHandler(ILogger<GetInvoicePdfQueryHandler> logger, IInvoicePdfGenerator pdfGenerator, IAppDbContext context)
: IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
                                            .AsNoTracking()
                                            .Where(i => i.Id == request.InvoiceId)
                                            .Include(i => i.InvoiceLineItems)
                                            .FirstOrDefaultAsync(cancellationToken);
        if (invoice is null)
        {
            logger.LogWarning("This Invoice Is Not Found  ID : {id}", request.InvoiceId);
            return ApplicationErrors.InvoiceNotFound;
        }

        try
        {
            var pdfBytes = pdfGenerator.Generate(invoice);
            return new InvoicePdfDto
            {
                Content = pdfBytes,
                FileName = $"Invoice-{request.InvoiceId}.Pdf",
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed To Generate PDF For Thi Invoice Id : {invoiceId}", request.InvoiceId);
            return ApplicationErrors.ErrorDuringGenerateInvoicePdf;
        }
    }
}
