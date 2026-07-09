using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/makes")]
[Tags("Vehicle Makes")] 
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)] 
public class VehicleMakesController(ISender sender, IOutputCacheStore outputCache) : ApiController
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [EndpointName("GetVehicleMakes")]
    [EndpointSummary("Retrieve all vehicle makes")]
    [EndpointDescription("Retrieves a complete list of all supported vehicle makes.")]
    [ProducesResponseType(typeof(List<VehicleMakeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(Duration = 7200, Tags = ["VehicleMakes"])]
    public async Task<IActionResult> GetVehicleMakes(CancellationToken ct)
    {
        var result = await sender.Send(new GetVehiclesMakesQuery(), ct);
        return result.Match(success => Ok(success), Problem);
    }

    [HttpGet("{makeId:guid}")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetModelsByMakeId")]
    [EndpointSummary("Retrieve vehicle models by make ID")]
    [EndpointDescription("Retrieves a list of all vehicle models associated with a specific vehicle make.")]
    [ProducesResponseType(typeof(List<VehicleModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(Duration = 7200, Tags = ["VehicleMakes"], VaryByRouteValueNames = ["makeId"])]
    public async Task<IActionResult> GetVehicleModelsByMakeId([FromRoute] Guid makeId, CancellationToken ct)
    {
        var result = await sender.Send(new GetVehiclesModelsByMakeIdQuery(makeId), ct);
        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost] 
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("CreateVehicleMake")]
    [EndpointSummary("Create a new vehicle make")]
    [EndpointDescription("Creates a new vehicle make along with its associated models.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] 
    public async Task<IActionResult> CreateNewVehicleMake([FromBody] CreateMakeRequest request, CancellationToken ct)
    {
        var models = request.Models.ConvertAll(n => new CreateVehicleModelCommand(n.Model));
        var result = await sender.Send(new CreateMakeCommand(request.Make, models), ct);
        
        if (result.IsSuccess)
        {
           await outputCache.EvictByTagAsync("VehicleMakes", ct);
        }
        return result.Match(success => StatusCode(StatusCodes.Status201Created, success), Problem); 
    }

    [HttpPut("{makeId:guid}")] 
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("UpdateVehicleMake")]
    [EndpointSummary("Update an existing vehicle make")]
    [EndpointDescription("Updates the details of a specific vehicle make and modifies its associated models.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMake([FromRoute] Guid makeId, [FromBody] UpdateMakeRequest request, CancellationToken ct)
    {
        var models = request.Models.ConvertAll(n => new UpdateModelCommand(n.ModelId, n.Model));
        var result = await sender.Send(new UpdateMakeCommand(makeId, request.Make, models), ct); 
        
        if (result.IsSuccess)
        {
           await outputCache.EvictByTagAsync("VehicleMakes", ct);
        }
        
        return result.Match(_ => NoContent(), Problem);
    }
}