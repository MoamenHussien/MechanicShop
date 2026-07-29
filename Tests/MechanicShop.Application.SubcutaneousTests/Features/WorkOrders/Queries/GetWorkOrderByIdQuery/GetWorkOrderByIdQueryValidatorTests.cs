using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetWorkOrderByIdQueryValidator _sut;

    public GetWorkOrderByIdQueryValidatorTests()
    {
        _sut = new GetWorkOrderByIdQueryValidator();
    }

    [Fact]
    public void GetWorkOrderByIdQueryValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var query = new global::GetWorkOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetWorkOrderByIdQueryValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var query = new global::GetWorkOrderByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "id");
    }
}
