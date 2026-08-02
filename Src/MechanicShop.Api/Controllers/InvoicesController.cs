using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/invoices")]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(Role.Manager))]
[Tags("Invoices")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class InvoicesController(ISender sender) : ApiController
{
    [HttpPost("workorders/{workOrderId:guid}")]
    [MapToApiVersion("1.0")]
    [EndpointName("IssueInvoiceForWorkOrder")]
    [EndpointSummary("Issues an invoice for a work order.")]
    [EndpointDescription("Creates a new invoice for the specified work order and returns the created invoice resource.")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IssueInvoice([FromRoute] Guid workOrderId, CancellationToken ct)
    {
        var result = await sender.Send(new IssueInvoiceCommand(workOrderId), ct);

        return result.Match(success => CreatedAtRoute("GetInvoice", new { version = "1.0", invoiceId = success.InvoiceId }, success), Problem);
    }

    [HttpGet("{invoiceId:guid}", Name = "GetInvoice")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetInvoice")]
    [EndpointSummary("Retrieves an invoice by ID.")]
    [EndpointDescription("Returns detailed information about the specified invoice.")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = (int)DurationInSeconds.OneHour, Tags = [CacheTags.Invoices], VaryByRouteValueNames = ["invoiceId"])]
    public async Task<IActionResult> GetInvoice([FromRoute] Guid invoiceId, CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoiceByIdQuery(invoiceId), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpPut("{invoiceId:guid}/payments")]
    [MapToApiVersion("1.0")]
    [EndpointName("SettleInvoice")]
    [EndpointSummary("Marks an invoice as paid.")]
    [EndpointDescription("Settles the specified invoice.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SettleInvoice([FromRoute] Guid invoiceId, CancellationToken ct)
    {
        var result = await sender.Send(new SettleInvoiceCommand(invoiceId), ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetInvoicePdf")]
    [EndpointSummary("Downloads the invoice as a PDF file.")]
    [EndpointDescription("Returns the invoice PDF file for the specified invoice ID.")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = (int)DurationInSeconds.TenMinutes, Tags = [CacheTags.Invoices], VaryByRouteValueNames = ["invoiceId"])]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetInvoicePdf([FromRoute] Guid invoiceId, CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoicePdfQuery(invoiceId), ct);

        return result.Match(success => File(success.Content!, "application/pdf", success.FileName), Problem);
    }
}
