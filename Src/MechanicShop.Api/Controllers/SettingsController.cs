using Asp.Versioning;
using MechanicShop.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace MechanicShop.Api.Controllers;

[Route("api/settings")]
[ApiVersionNeutral]
[Tags("Settings")]
public sealed class SettingsController(IOptions<AppSettings> options) : ApiController
{
    [HttpGet("operating-hours")]
    [ProducesResponseType(typeof(OperatingHoursResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the application's operating hours.")]
    [EndpointDescription("Returns the current configured opening and closing times.")]
    [EndpointName("GetOperatingHours")]
    [OutputCache(Duration = (int)DurationInSeconds.OneDay)]
    public IActionResult GetOperatingHours()
    {
        return Ok(new OperatingHoursResponse(options.Value.OpeningTime, options.Value.ClosingTime));
    }
}
