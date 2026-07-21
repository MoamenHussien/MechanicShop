using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

public class GetWorkOrderStatsQueryValidatorTests
{
    private readonly GetWorkOrderStatsQueryValidator _sut;

    public GetWorkOrderStatsQueryValidatorTests()
    {
        _sut = new GetWorkOrderStatsQueryValidator();
    }

    [Fact]
    public void GetWorkOrderStatsValidator_ShouldSucceed_WithValidDate()
    {
        // Arrange
        var query = new GetWorkOrderStatsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetWorkOrderStatsValidator_ShouldFail_WithEmptyDate()
    {
        // Arrange
        var query = new GetWorkOrderStatsQuery(default);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date");
    }
}
