using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdCommandValidatorTests
{
    private readonly GetUserByIdCommandValidator _sut;

    public GetUserByIdCommandValidatorTests()
    {
        _sut = new GetUserByIdCommandValidator();
    }

    private static GetUserByIdCommand CreateCommand(Guid? id = null)
    {
        return new(id ?? Guid.NewGuid());
    }

    [Fact]
    public void GetUserByIdValidator_ShouldSucceed_WithValidId()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetUserByIdValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = CreateCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "id");
    }
}
