using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Contracts.Requests.Labors;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class LaborsControllerTests
{
    private readonly WebAppFactory _webAppFactory;
    private readonly AppHttpClient _client;
    private readonly IAppDbContext _context;

    public LaborsControllerTests(WebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _client = webAppFactory.CreateAppHttpClient();
        _context = webAppFactory.CreateAppDbContext();
    }

    // ========================================================================
    // GET /api/v{version}/labors
    // ========================================================================

    [Fact]
    public async Task GetLabors_WithValidToken_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLabors_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/labors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // POST /api/v{version}/labors
    // ========================================================================

    [Fact]
    public async Task CreateLabor_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new RegisterLaborRequestContract
        {
            Email = "newlabor@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "Labor",
            Roles = ["Labor"]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/labors", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateLabor_WithDuplicateEmail_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new RegisterLaborRequestContract
        {
            Email = "duplicatelabor@example.com",
            Password = "Password123!",
            FirstName = "Dup",
            LastName = "Labor",
            Roles = ["Labor"]
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/v1.0/labors", request);
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await _client.PostAsJsonAsync("/api/v1.0/labors", request);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateLabor_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var request = new RegisterLaborRequestContract
        {
            Email = "forbiddenlabor@example.com",
            Password = "Password123!",
            FirstName = "Forbidden",
            LastName = "Labor",
            Roles = ["Labor"]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/labors", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/labors/{laborid}/info
    // ========================================================================

    [Fact]
    public async Task UpdateLaborInfo_WithValidRequest_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync();
        Assert.NotNull(labor);

        var request = new UpdateLaborInfoRequest
        {
            FirstName = "Updated",
            LastName = "LaborInfo",
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{labor!.Id}/info", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLaborInfo_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var request = new UpdateLaborInfoRequest { FirstName = "FirstName", LastName = "LastName", IsActive = true };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{nonExistentId}/info", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLaborInfo_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync();
        Assert.NotNull(labor);

        var request = new UpdateLaborInfoRequest { FirstName = "FirstName", LastName = "LastName", IsActive = true };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{labor!.Id}/info", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/labors/{laborid}/permissions
    // ========================================================================

    [Fact]
    public async Task UpdateLaborPermissions_WithValidRequest_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync(l => l.Id == TestUsers.Labor02.Id);
        Assert.NotNull(labor);

        var request = new UpdateLaborPermissionsRequest
        {
            Roles = ["Labor"],
            Claims = []
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{labor!.Id}/permissions", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLaborPermissions_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var request = new UpdateLaborPermissionsRequest { Roles = ["Labor"], Claims = [] };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{nonExistentId}/permissions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLaborPermissions_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync();
        Assert.NotNull(labor);

        var request = new UpdateLaborPermissionsRequest { Roles = ["Labor"], Claims = [] };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/labors/{labor!.Id}/permissions", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v{version}/labors/details
    // ========================================================================

    [Fact]
    public async Task GetEmployeeDetails_WithManagerRole_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var details = await response.Content.ReadFromJsonAsync<List<EmployeeDetailDto>>();

        Assert.NotNull(details);
        Assert.NotEmpty(details);
    }

    [Fact]
    public async Task GetEmployeeDetails_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/labors/details");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/labors/{laborid}/reset-password
    // ========================================================================

    [Fact]
    public async Task ResetLaborPassword_WithManagerRole_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync(l => l.Id == TestUsers.Labor01.Id);
        Assert.NotNull(labor);

        var response = await _client.PutAsJsonAsync<object?>($"/api/v1.0/labors/{labor!.Id}/reset-password", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ResetLaborPassword_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var labor = await _context.Employees.FirstOrDefaultAsync();
        Assert.NotNull(labor);

        var response = await _client.PutAsJsonAsync<object?>($"/api/v1.0/labors/{labor!.Id}/reset-password", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/labors/update-password
    // ========================================================================

    [Fact]
    public async Task UpdateUserPassword_WithValidCredentials_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var request = new UpdateLaborPasswordRequest(TestUsers.Labor01.Email!, "NewPassword123!");

        var response = await _client.PutAsJsonAsync("/api/v1.0/labors/update-password", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
