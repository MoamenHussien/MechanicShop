using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _sut;

    public GetRepairTaskByIdQueryValidatorTests()
    {
        _sut = new GetRepairTaskByIdQueryValidator();
    }

    [Fact]
    public void GetRepairTaskByIdQueryValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetRepairTaskByIdQueryValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var query = new GetRepairTaskByIdQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "id");
    }
}