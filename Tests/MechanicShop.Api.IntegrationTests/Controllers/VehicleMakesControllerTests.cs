using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class VehicleMakesControllerTests
{
    private readonly WebAppFactory _webAppFactory;
    private readonly AppHttpClient _client;
    private readonly IAppDbContext _context;

    public VehicleMakesControllerTests(WebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _client = webAppFactory.CreateAppHttpClient();
        _context = webAppFactory.CreateAppDbContext();
    }

    // ========================================================================
    // GET /api/v{version}/makes
    // ========================================================================
    [Fact]
    public async Task GetVehicleMakes_WithValidToken_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/makes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicleMakes_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/makes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v{version}/makes/{makeId:guid}
    // ========================================================================
    [Fact]
    public async Task GetVehicleModelsByMakeId_WithValidId_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var make = await _context.VehicleMakes.FirstOrDefaultAsync();
        Assert.NotNull(make);

        var response = await _client.GetAsync($"/api/v1.0/makes/{make!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicleModelsByMakeId_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/makes/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // POST /api/v{version}/makes
    // ========================================================================
    [Fact]
    public async Task CreateNewVehicleMake_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new CreateMakeRequest
        {
            Make = "Test Make",
            Models = [new() { Model = "Test Model A" }],
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/makes", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            await _context.VehicleModels.Where(m => m.Model == "Test Model A").ExecuteDeleteAsync();
            await _context.VehicleMakes.Where(m => m.Make == "Test Make").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateNewVehicleMake_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var request = new CreateMakeRequest
        {
            Make = "Forbidden Make",
            Models = [new() { Model = "Forbidden Model" }],
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/makes", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/makes/{makeId:guid}
    // ========================================================================
    [Fact]
    public async Task UpdateMake_WithValidRequest_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var createRequest = new CreateMakeRequest
        {
            Make = "Make To Update",
            Models = [new() { Model = "Model To Update" }],
        };

        try
        {
            var createResponse = await _client.PostAsJsonAsync("/api/v1.0/makes", createRequest);
            createResponse.EnsureSuccessStatusCode();

            var content = await createResponse.Content.ReadFromJsonAsync<Guid>();
            var makeId = content;

            var updateRequest = new UpdateMakeRequest
            {
                Make = "Updated Make",
                Models = [new() { Model = "Updated Model Name" }],
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/v1.0/makes/{makeId}", updateRequest);

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }
        finally
        {
            await _context.VehicleModels.Where(m => m.Model == "Model To Update" || m.Model == "Updated Model Name").ExecuteDeleteAsync();
            await _context.VehicleMakes.Where(m => m.Make == "Make To Update" || m.Make == "Updated Make").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateMake_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var request = new UpdateMakeRequest
        {
            Make = "NonExistent Update",
            Models = [new() { Model = "Model Update" }],
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/makes/{nonExistentId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMake_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var make = await _context.VehicleMakes.FirstOrDefaultAsync();
        Assert.NotNull(make);

        var request = new UpdateMakeRequest
        {
            Make = "Forbidden Update",
            Models = [new() { Model = "Model Update" }],
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/makes/{make!.Id}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
