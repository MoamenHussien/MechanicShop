using System.Net;
using System.Net.Http.Json;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Contracts.Responses;
using Xunit;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class SettingsControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();

    // ========================================================================
    // GET /api/settings/operating-hours
    // ========================================================================

    [Fact]
    public async Task GetOperatingHours_ShouldReturnOkWithConfiguredHours()
    {
        var response = await _client.GetAsync("/api/settings/operating-hours");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var operatingHours = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();

        Assert.NotNull(operatingHours);
        Assert.Equal(new TimeOnly(9, 0), operatingHours!.OpeningTime);
        Assert.Equal(new TimeOnly(18, 0), operatingHours.ClosingTime);
    }
}
