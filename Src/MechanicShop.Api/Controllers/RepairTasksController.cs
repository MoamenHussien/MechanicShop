using Asp.Versioning;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Contracts.Requests.RepairTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/repair-tasks")]
[ApiVersion("1.0")]
[Tags("RepairTasks")]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class RepairTasksController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Retrieves all repair tasks.")]
    [EndpointDescription("Returns a list of all repair tasks available in the system.")]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = (int)DurationInSeconds.OneDay, Tags = [CacheTags.RepairTasks])]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTasksQuery(), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpGet("{repairTaskId:guid}", Name = "GetById")]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Retrieves a repair task by ID.")]
    [EndpointDescription("Returns detailed information for the specified repair task if it exists.")]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = (int)DurationInSeconds.OneDay, Tags = [CacheTags.RepairTasks], VaryByRouteValueNames = ["repairTaskId"])]
    public async Task<IActionResult> GetById(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Creates a new repair task.")]
    [EndpointDescription("Creates a repair task and optionally includes parts.")]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateRepairTaskRequest request, CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(p => new CreateRepairTaskPartCommand(p.Name, p.Cost, p.Quantity));

        var command = new CreateRepairTaskCommand(
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)(int)request.EstimatedDurationInMins!.Value,
            parts);

        var result = await sender.Send(command, ct);

        return result.Match(success => CreatedAtAction(nameof(GetById), new { repairTaskId = success.RepairTaskId }, success), Problem);
    }

    [HttpPut("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Updates an existing repair task.")]
    [EndpointDescription("Updates a repair task and its associated parts.")]
    [EndpointName("UpdateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid repairTaskId, [FromBody] UpdateRepairTaskRequest request, CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(p => new UpdatePartCommand(p.PartId, p.Name, p.Cost, p.Quantity));

        var command = new UpdateRepairTaskCommand(
            repairTaskId,
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)request.EstimatedDurationInMins,
            parts);

        var result = await sender.Send(command, ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Removes a repair task.")]
    [EndpointDescription("Deletes the specified repair task from the system.")]
    [EndpointName("RemoveRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRepairTaskCommand(repairTaskId), ct);

        return result.Match(_ => NoContent(), Problem);
    }
}
