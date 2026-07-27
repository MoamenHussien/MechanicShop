using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Contracts.Requests.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
[Authorize]
[Tags("Customers")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class CustomersController(ISender sender) : ApiController
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Tags = [CacheTags.Customers])]
    [EndpointName("GetCustomers")]
    [EndpointSummary("Retrieve All Customers")]
    [EndpointDescription("Returns all customers along with their vehicles")]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomersQuery(), ct);
        return result.Match(success => Ok(success), Problem);
    }

    [HttpGet("{customerId:guid}", Name = "GetCustomerById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Tags = [CacheTags.Customers], VaryByRouteValueNames = ["customerId"])]
    [EndpointName("GetCustomerById")]
    [EndpointSummary("Get Customer Info By Id")]
    [EndpointDescription("Get Customer Info By Id With His vehicles, If Found")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), ct);
        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("CreateNewCustomer")]
    [EndpointSummary("Create New Customer")]
    [EndpointDescription("Create New Customer With His Vehicles")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var vehicles = request.Vehicles.ConvertAll(n => new CreateVehicleCommand(n.Year, n.LicensePlate, n.ModelId));
        var result = await sender.Send(new CreateCustomerCommand(request.Name, request.Email, request.PhoneNumber, vehicles), ct);

        return result.Match(success => CreatedAtRoute("GetCustomerById", new { version = "1.0", customerId = success.CustomerId }, success), Problem);
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("UpdateCustomer")]
    [EndpointSummary("Updates an existing customer.")]
    [EndpointDescription("Updates customer information including associated vehicles.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomer([FromRoute] Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var vehicles = request.Vehicles.ConvertAll(n => new UpdateVehicleCommand(n.VehicleId, n.Year, n.LicensePlate, n.ModelId));
        var result = await sender.Send(new UpdateCustomerCommand(customerId, request.Name, request.Email, request.PhoneNumber, vehicles), ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("DeleteCustomer")]
    [EndpointSummary("Delete an existing customer.")]
    [EndpointDescription("Delete customer information including associated vehicles.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCustomer([FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCustomerCommand(customerId), ct);

        return result.Match(_ => NoContent(), Problem);
    }
}