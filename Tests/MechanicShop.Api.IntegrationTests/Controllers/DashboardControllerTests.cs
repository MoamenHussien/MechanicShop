using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Tests.Common.Security;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class DashboardControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    // ========================================================================
    // GET /api/v1.0/dashboard/stats
    // ========================================================================

    [Fact]
    public async Task GetTodayStats_WithDefaultDate_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/dashboard/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result!.Date);
    }

    [Fact]
    public async Task GetTodayStats_WithSpecificDate_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var specificDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={specificDate:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
        Assert.Equal(specificDate, result!.Date);
    }

    [Fact]
    public async Task GetTodayStats_WithNoWorkOrders_ShouldReturnZeroStats()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        // Use a far future date where no work orders exist
        var emptyDate = new DateOnly(2099, 1, 1);

        var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={emptyDate:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
        Assert.Equal(emptyDate, result!.Date);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Scheduled);
        Assert.Equal(0, result.InProgress);
        Assert.Equal(0, result.Completed);
        Assert.Equal(0, result.Cancelled);
        Assert.Equal(0, result.TotalRevenue);
        Assert.Equal(0, result.UniqueVehicles);
        Assert.Equal(0, result.UniqueCustomers);
    }

    [Fact]
    public async Task GetTodayStats_WithExistingWorkOrders_ShouldReturnAccurateStats()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .ForToday()
            .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(TestUsers.Labor01.Id)
            .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.GetAsync($"/api/v1.0/dashboard/stats?date={today:yyyy-MM-dd}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

            Assert.NotNull(result);
            Assert.Equal(today, result!.Date);
            Assert.True(result.Total > 0);
        }
        finally
        {
            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetTodayStats_AsLabor_ShouldReturnOk()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/dashboard/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TodayWorkOrderStatsDto>();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTodayStats_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/dashboard/stats");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
