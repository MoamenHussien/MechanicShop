using Asp.Versioning;
using MechanicShop.Application.Features.Labors.Queries;
using MechanicShop.Contracts.Requests.Labors;
using MechanicShop.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/labors")]
[ApiVersion("1.0")]
[Tags("Labors")]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class LaborsController(ISender sender, IOutputCacheStore outputCache) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Retrieves the list of available labor definitions.")]
    [EndpointDescription("Returns all labor records associated with the system. Accessible only to authorized users.")]
    [EndpointName("GetLabors")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = (int)DurationInSeconds.OneDay, Tags = ["Labors"])]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetLaborsQuery(), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpGet("details")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(List<EmployeeDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Retrieves detailed information for all employees.")]
    [EndpointDescription("Returns complete employee details including name, email, roles, and status. Accessible only to Managers.")]
    [EndpointName("GetEmployeeDetails")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetEmployeeDetails(CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeDetailsQuery(), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Creates a new labor definition.")]
    [EndpointDescription("Registers a new labor entity with the specified information and returns the created identifier. Accessible only to Managers.")]
    [EndpointName("CreateLabor")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] RegisterLaborRequestContract request, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterLaborCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Roles, []), ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("Labors", ct);
        }

        return result.Match(success => StatusCode(StatusCodes.Status201Created, success), Problem);
    }

    [HttpPut("{laborid:guid}/info")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates labor basic information.")]
    [EndpointDescription("Modifies the first name, last name, and active status for a specific labor. Accessible only to Managers.")]
    [EndpointName("UpdateLaborInfo")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateLaborInfo([FromRoute] Guid laborid, [FromBody] UpdateLaborInfoRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateLaborInfoCommand(laborid, request.FirstName, request.LastName, request.IsActive), ct);
        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("Labors", ct);
        }

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{laborid:guid}/permissions")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates labor permissions.")]
    [EndpointDescription("Modifies the roles and claims assigned to a specific labor. Accessible only to Managers.")]
    [EndpointName("UpdateLaborPermissions")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateLaborPermissions([FromRoute] Guid laborid, [FromBody] UpdateLaborPermissionsRequest request, CancellationToken ct)
    {
        var command = new UpdateLaborPermissionsCommand(laborid, request.Roles, request.Claims ?? []);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("Labors", ct);
        }

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPost("{laborid:guid}/reset-password")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(Success), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Resets the password of a specific labor.")]
    [EndpointDescription("Resets the password for the labor with the specified ID to its email address. Accessible only to Managers.")]
    [EndpointName("ResetLaborPassword")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ResetLaborPassword([FromRoute] Guid laborid, CancellationToken ct)
    {
        var result = await sender.Send(new ResetLaborPasswordCommand(laborid), ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("update-password")]
    [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Labor)}")]
    [ProducesResponseType(typeof(Success), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Updates the password of a specific user.")]
    [EndpointDescription("Updates the password of the user. Accessible only to Managers and Labors.")]
    [EndpointName("UpdateUserPassword")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateLaborPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateLaborPasswordCommand(request.NewPassword, request.CurrentPassword), ct);

        return result.Match(_ => NoContent(), Problem);
    }


}