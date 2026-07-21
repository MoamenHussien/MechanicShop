using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Common;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RepairTasksControllerTests
{
    private readonly WebAppFactory _webAppFactory;
    private readonly AppHttpClient _client;
    private readonly IAppDbContext _context;

    public RepairTasksControllerTests(WebAppFactory webAppFactory)
    {
        _webAppFactory = webAppFactory;
        _client = webAppFactory.CreateAppHttpClient();
        _context = webAppFactory.CreateAppDbContext();
    }

    // ========================================================================
    // GET /api/v{version}/repair-tasks
    // ========================================================================

    [Fact]
    public async Task GetRepairTasks_WithValidToken_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/repair-tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTasks_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/repair-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ========================================================================
    // GET /api/v{version}/repair-tasks/{repairTaskId}
    // ========================================================================

    [Fact]
    public async Task GetRepairTaskById_WithValidId_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var task = await _context.RepairTasks.FirstOrDefaultAsync();
        Assert.NotNull(task);

        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{task!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRepairTaskById_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/repair-tasks/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ========================================================================
    // POST /api/v{version}/repair-tasks
    // ========================================================================

    [Fact]
    public async Task CreateRepairTask_WithValidRequest_ShouldReturnCreated()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskRequest
        {
            Name = "New Integration Task",
            LaborCost = 150.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min120,
            Parts =
            [
                new CreateRepairTaskPartRequest { Name = "Part A", Cost = 50.00m, Quantity = 2 }
            ]
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            await _context.Parts.Where(p => p.Name == "Part A").ExecuteDeleteAsync();
            await _context.RepairTasks.Where(rt => rt.Name == "New Integration Task").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateRepairTask_WithDuplicateName_ShouldReturnConflict()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskRequest
        {
            Name = "Duplicate Task",
            LaborCost = 100.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min60,
            Parts =
            [
                new CreateRepairTaskPartRequest { Name = "Part B", Cost = 10.00m, Quantity = 1 }
            ]
        };

        try
        {
            var firstResponse = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);
            firstResponse.EnsureSuccessStatusCode();

            var secondResponse = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }
        finally
        {
            await _context.Parts.Where(p => p.Name == "Part B").ExecuteDeleteAsync();
            await _context.RepairTasks.Where(rt => rt.Name == "Duplicate Task").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var request = new CreateRepairTaskRequest
        {
            Name = "Forbidden Task",
            LaborCost = 100.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min60,
            Parts = [ new() { Name = "Valid Part", Cost = 10m, Quantity = 1 } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // PUT /api/v{version}/repair-tasks/{repairTaskId}
    // ========================================================================

    [Fact]
    public async Task UpdateRepairTask_WithValidRequest_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var createRequest = new CreateRepairTaskRequest
        {
            Name = "Task To Update",
            LaborCost = 50.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min30,
            Parts =
            [
                new CreateRepairTaskPartRequest { Name = "Part C", Cost = 20.00m, Quantity = 1 }
            ]
        };

        try
        {
            var createResponse = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", createRequest);
            createResponse.EnsureSuccessStatusCode();
            
            // Extract the created ID from the Location header
            var location = createResponse.Headers.Location;
            Assert.NotNull(location);
            var idString = location!.Segments.Last();
            var taskId = Guid.Parse(idString);

            var updateRequest = new UpdateRepairTaskRequest
            {
                Name = "Updated Task",
                LaborCost = 75.00m,
                EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min60,
                Parts =
                [
                    new UpdateRepairTaskPartRequest { Name = "Updated Part C", Cost = 25.00m, Quantity = 2 }
                ]
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{taskId}", updateRequest);

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }
        finally
        {
            await _context.Parts.Where(p => p.Name == "Part C" || p.Name == "Updated Part C").ExecuteDeleteAsync();
            await _context.RepairTasks.Where(rt => rt.Name == "Task To Update" || rt.Name == "Updated Task").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTask_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var request = new UpdateRepairTaskRequest
        {
            Name = "NonExistent Update",
            LaborCost = 100.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min60,
            Parts = [ new() { Name = "Valid Part", Cost = 10m, Quantity = 1 } ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{nonExistentId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var task = await _context.RepairTasks.FirstOrDefaultAsync();
        Assert.NotNull(task);

        var request = new UpdateRepairTaskRequest
        {
            Name = "Forbidden Update",
            LaborCost = 100.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min60,
            Parts = [ new() { Name = "Valid Part", Cost = 10m, Quantity = 1 } ]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/repair-tasks/{task!.Id}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ========================================================================
    // DELETE /api/v{version}/repair-tasks/{repairTaskId}
    // ========================================================================

    [Fact]
    public async Task DeleteRepairTask_WithValidId_ShouldReturnNoContent()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var createRequest = new CreateRepairTaskRequest
        {
            Name = "Task To Delete",
            LaborCost = 25.00m,
            EstimatedDurationInMins = MechanicShop.Contracts.Common.RepairDurationInMinutes.Min30,
            Parts = [ new() { Name = "Valid Part", Cost = 10m, Quantity = 1 } ]
        };

        try
        {
            var createResponse = await _client.PostAsJsonAsync("/api/v1.0/repair-tasks", createRequest);
            createResponse.EnsureSuccessStatusCode();

            var location = createResponse.Headers.Location;
            Assert.NotNull(location);
            var idString = location!.Segments.Last();
            var taskId = Guid.Parse(idString);

            var deleteResponse = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{taskId}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
        finally
        {
            await _context.RepairTasks.Where(rt => rt.Name == "Task To Delete").ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteRepairTask_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRepairTask_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);
        _client.SetAuthorizationHeader(token);

        var task = await _context.RepairTasks.FirstOrDefaultAsync();
        Assert.NotNull(task);

        var response = await _client.DeleteAsync($"/api/v1.0/repair-tasks/{task!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
