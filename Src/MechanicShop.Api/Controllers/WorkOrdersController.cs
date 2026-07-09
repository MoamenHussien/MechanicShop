using Asp.Versioning;
using MechanicShop.Contracts.Requests.WorkOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/workorders")]
[ApiVersion("1.0")]
[Authorize]
[Tags("WorkOrders")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class WorkOrdersController(ISender sender, IOutputCacheStore outputCache) : ApiController
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [EndpointName("GetWorkOrders")]
    [EndpointSummary("Retrieves a paginated list of work orders.")]
    [EndpointDescription("Supports filtering by date range, status, vehicle, labor, spot, and searching by term. Pagination and sorting are supported.")]
    [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [OutputCache(Duration = 60, Tags = ["WorkOrders"], VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> Get
    ([FromQuery] WorkOrderFilterRequest filters, [FromQuery] PageRequest pageRequest, CancellationToken ct)
    {
        if (pageRequest.Page <= 0)
        {
            return BadRequest("Page must be greater than 0");
        }

        if (pageRequest.PageSize <= 0 || pageRequest.PageSize > 100)
        {
            return BadRequest("PageSize must be between 1 and 100");
        }

        var query = new GetAllWorkOrderQuery(
            pageRequest.Page,
            pageRequest.PageSize,
            filters.SearchTerm,
            filters.SortColumn,
            filters.SortDirection,
            filters.State is not null ? (WorkOrderState)(int)filters.State : null,
            filters.VehicleId,
            filters.LaborId,
            filters.StartDateFrom,
            filters.StartDateTo,
            filters.EndDateFrom,
            filters.EndDateTo,
            filters.Spot is not null ? (Spot)(int)filters.Spot : null);

        var result = await sender.Send(query, ct);

        return result.Match(success => Ok(success),Problem);
    }

    [HttpGet("{workOrderId:guid}", Name = "GetWorkOrderById")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetWorkOrderById")]
    [EndpointSummary("Retrieves a work order by its ID.")]
    [EndpointDescription("Returns detailed information about the specified work order if it exists.")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(Duration = 60, Tags = ["WorkOrders"], VaryByRouteValueNames = ["workOrderId"])]
    public async Task<IActionResult> GetById([FromRoute] Guid workOrderId, CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkOrderByIdQuery(workOrderId), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = nameof(Role.Manager))]
    [EndpointName("CreateWorkOrder")]
    [EndpointSummary("Creates a new work order.")]
    [EndpointDescription("Creates a new work order for a vehicle, specifying labor, tasks, and other required information.")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateWorkOrderCommand(
            request.LaborId,
            request.VehicleId,
            (Spot)(int)request.Spot,
            request.StartAtUtc,
            request.RepairTaskIds
            ),
            ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match(success => CreatedAtRoute( routeName: "GetWorkOrderById", routeValues: new { version = "1.0", workOrderId = success.WorkOrderId },value: success),Problem);
    }

    [HttpPut("{workOrderId:guid}/relocation")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = nameof(Role.Manager))]
    [EndpointName("RescheduleWorkOrder")]
    [EndpointSummary("Relocates a work order to a new time and spot.")]
    [EndpointDescription("Updates the scheduled time and assigned bay for a work order. Only users with the Manager role can perform this action.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Relocate([FromRoute] Guid workOrderId, [FromBody] RelocateWorkOrderRequest request, CancellationToken ct)
    {
        var command = new RelocateWorkOrderCommand(
            workOrderId,
            request.NewStartAtUtc,
            (Spot)(int)request.NewSpot);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match( _ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/labor")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = nameof(Role.Manager))]
    [EndpointName("AssignLaborToWorkOrder")]
    [EndpointSummary("Assigns a labor to a work order.")]
    [EndpointDescription("Associates a labor definition with a specific work order. Only managers can perform this operation.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReAssignLabor([FromRoute] Guid workOrderId, [FromBody] AssignLaborRequest request, CancellationToken ct)
    {
        var command = new ReAssignLaborCommand(workOrderId, request.LaborId.ToGuid().Value);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match( _ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/state")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Labor)}", Policy = "SelfScopedWorkOrderAccess")]
    [EndpointName("UpdateWorkOrderState")]
    [EndpointSummary("Changes the state of a work order.")]
    [EndpointDescription("Updates the current state of the specified work order. Only users with the Manager role are authorized.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateState([FromRoute] Guid workOrderId, [FromBody] UpdateWorkOrderStateRequest request, CancellationToken ct)
    {
        var command = new UpdateWorkOrderStateCommand(
            workOrderId,
            (WorkOrderState)(int)request.State);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/repair-task")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("UpdateRepairTasks")]
    [EndpointSummary("Updates repair tasks for a work order.")]
    [EndpointDescription("Modifies the list of repair tasks associated with a specific work order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRepairTasks([FromRoute] Guid workOrderId, [FromBody] ModifyRepairTaskRequest request, CancellationToken ct)
    {
        var command = new UpdateWorkOrderRepairTasksCommand(workOrderId, request.RepairTaskIds);

        var result = await sender.Send(command, ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match( _ => NoContent(),Problem);
    }

    [HttpDelete("{workOrderId:guid}")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = nameof(Role.Manager))]
    [EndpointName("DeleteWorkOrder")]
    [EndpointSummary("Deletes a work order.")]
    [EndpointDescription("Deletes the specified work order permanently. Only users with the Manager role are authorized.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid workOrderId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteWorkOrderCommand(workOrderId), ct);

        if (result.IsSuccess)
        {
            await outputCache.EvictByTagAsync("WorkOrders", ct);
        }

        return result.Match(_ => NoContent() ,Problem);
    }

    [HttpGet("schedule/{date}")]
    [MapToApiVersion("1.0")]
    [Authorize]
    [EndpointName("GetDailySchedule")]
    [EndpointSummary("Retrieves the schedule for a given day.")]
    [EndpointDescription("Returns a schedule view for the specified date. If no date is provided, today's schedule is returned. You can optionally filter by labor ID.")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [OutputCache(Duration = 30, Tags = ["WorkOrders"], VaryByRouteValueNames = ["date"], VaryByQueryKeys = ["laborId"], VaryByHeaderNames = ["X-TimeZone"])] 
       public async Task<IActionResult> GetSchedule([FromRoute] DateOnly? date, [FromQuery] Guid? laborId, [FromHeader(Name = "X-TimeZone")] string? tz, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tz))
        {
            return Problem(
                detail: "Missing time zone in 'X-TimeZone' header.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Time Zone Required");
        }

        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
        }
        catch
        {
            return Problem(
                detail: $"Invalid or unknown time zone: '{tz}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Time Zone");
        }

        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await sender.Send(new GetDailyScheduleQuery(scheduleDate, timeZone, laborId), ct);

        return result.Match(success => Ok(success),Problem);
    }
}