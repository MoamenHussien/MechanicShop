using System.Security.Claims;
using Asp.Versioning;
using MechanicShop.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

[Route("identity")]
[ApiVersionNeutral]
[Tags("Identity")]
public class IdentityController(ISender sender) : ApiController
{
    [HttpPost("token/generate")]
    [EndpointName("GenerateToken")]
    [EndpointSummary("Signs in a user and issues authentication tokens.")]
    [EndpointDescription("Validates the user's credentials, returns a JWT access token in the response body, and stores the refresh token in a secure HttpOnly cookie for future token refresh requests.")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateToaken([FromBody] GenerateTokenCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);
        return result.Match(Success => Ok(Success), Problem);
    }

    [HttpPost("token/refresh-token")]
    [EndpointName("RefreshToken")]
    [EndpointSummary("Refreshes the access token using the refresh token stored in the HttpOnly cookie.")]
    [EndpointDescription("Generates a new access token using the expired access token from the request body and the refresh token stored in an HttpOnly cookie.")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("current-user/claims")]
    [Authorize]
    [EndpointName("GetCurrentUserClaims")]
    [EndpointSummary("Gets the current authenticated user's info.")]
    [EndpointDescription("Returns user information for the currently authenticated user based on the access token.")]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [OutputCache(PolicyName = nameof(Policies.PerUserAuthCache), Duration = 120)]
    public async Task<IActionResult> GetCurrentUserInfo(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var result = await sender.Send(new GetUserByIdCommand(userIdClaim.ToGuid().Value), ct);

        return result.Match(success => Ok(success), Problem);
    }

    [HttpPost("logout")]
    [Authorize]
    [EndpointName("Logout")]
    [EndpointSummary("Logs out the authenticated user.")]
    [EndpointDescription("Revokes the current refresh token, removes the refresh token cookie, and ends the authenticated session.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await sender.Send(new LogoutCommand(), cancellationToken);

        return NoContent();
    }

    [HttpGet("assignable-roles")]
    [Authorize(Roles = nameof(Role.Manager))]
    [EndpointName("AssignableRoles")]
    [EndpointSummary("Gets all assignable roles.")]
    [EndpointDescription("Returns all system roles except the Manager role.")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [OutputCache(PolicyName = nameof(Policies.SharedAuthCache), Duration = 120)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllSystemRolesQuery(), ct);

        return result.Match(success => Ok(success), Problem);
    }
}
