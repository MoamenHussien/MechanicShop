using System.Collections.Specialized;
using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MechanicShop.Application.Common.Constants;
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
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache) ,Tags = [CacheTags.WorkOrders] ,Duration = (int)DurationInSeconds.FiveMinutes , VaryByQueryKeys = ["date"], VaryByHeaderNames = ["X-TimeZone"])]
    public async Task<IActionResult> GetTodayStats([FromQuery] DateOnly? date, [FromHeader(Name = "X-TimeZone")] string? tz, CancellationToken ct)
    {
        TimeZoneInfo timeZone = TimeZoneInfo.Local;
        if (!string.IsNullOrWhiteSpace(tz))
        {
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch
            {
                // Fallback to local
            }
        }

        var statsDate = date ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone)); 
        
        var result = await sender.Send(new GetWorkOrderStatsQuery(statsDate, timeZone), ct);
        
        return result.Match(success => Ok(success), Problem);
    }
}



