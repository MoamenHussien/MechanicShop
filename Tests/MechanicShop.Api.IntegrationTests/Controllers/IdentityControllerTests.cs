using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Tests.Common.Security;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IdentityControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    // ========================================================================
    // POST /identity/token/generate
    // ========================================================================

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ShouldReturnOk()
    {
        var request = new GenerateTokenCommand(TestUsers.Manager.Email!, TestUsers.Manager.Email!);

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse!.AccessToken));
    }

    [Fact]
    public async Task GenerateToken_WithInvalidPassword_ShouldReturnConflict()
    {
        var request = new GenerateTokenCommand(TestUsers.Manager.Email!, "WrongPassword123!");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GenerateToken_WithNonExistentEmail_ShouldReturnNotFound()
    {
        var request = new GenerateTokenCommand("nonexistentuser@example.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GenerateToken_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        var request = new GenerateTokenCommand("invalid-email-format", "Password123!");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateToken_WithShortPassword_ShouldReturnBadRequest()
    {
        var request = new GenerateTokenCommand("validemail@example.com", "short");

        var response = await _client.PostAsJsonAsync("/identity/token/generate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ========================================================================
    // POST /identity/token/refresh-token
    // ========================================================================

    [Fact]
    public async Task RefreshToken_WithEmptyAccessToken_ShouldReturnBadRequest()
    {
        var request = new RefreshTokenCommand(string.Empty);

        var response = await _client.PostAsJsonAsync("/identity/token/refresh-token", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidAccessToken_ShouldReturnForbidden()
    {
        var request = new RefreshTokenCommand("invalid.jwt.token");

        var response = await _client.PostAsJsonAsync("/identity/token/refresh-token", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // GET /identity/current-user/claims
    // ========================================================================

    [Fact]
    public async Task GetCurrentUserInfo_WithValidToken_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/identity/current-user/claims");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userDto = await response.Content.ReadFromJsonAsync<AppUserDto>();

        Assert.NotNull(userDto);
        Assert.Equal(TestUsers.Manager.Id, userDto!.UserId);
        Assert.Equal(TestUsers.Manager.Email, userDto.Email);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/identity/current-user/claims");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // GET /identity/assignable-roles
    // ========================================================================

    [Fact]
    public async Task GetRoles_WithManagerRole_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/identity/assignable-roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var roles = await response.Content.ReadFromJsonAsync<List<string>>();

        Assert.NotNull(roles);
        Assert.NotEmpty(roles);
    }

    [Fact]
    public async Task GetRoles_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/identity/assignable-roles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // POST /identity/logout
    // ========================================================================

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.PostAsJsonAsync<object?>("/identity/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync<object?>("/identity/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
