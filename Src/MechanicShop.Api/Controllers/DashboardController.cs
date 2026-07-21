using System.Collections.Specialized;
using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicShop.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard")]
[Tags("Dashboard")]
[ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
public class DashboardController(ISender sender) : ApiController
{
    [HttpGet("stats")]
    [MapToApiVersion("1.0")]
    [EndpointName("GetTodayStats")] 
    [EndpointSummary("Retrieve daily workshop statistics and KPIs.")] 
    [EndpointDescription("Generates a comlrehensive daily statistical report including order statuses, financial metrics (revenue, profit, costs), and operational ratios for the specified date.")] 
    [ProducesResponseType(typeof(TodayWorkOrderStatsDto), StatusCodes.Status200OK)]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache) ,Duration = (int)DurationInSeconds.OneMinute , VaryByQueryKeys = ["date"])]
    public async Task<IActionResult> GetTodayStats([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var statsDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow); 
        
        var result = await sender.Send(new GetWorkOrderStatsQuery(statsDate), ct);
        
        return result.Match(success => Ok(success), Problem);
    }
}



