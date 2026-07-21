using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryValidatorTests
{
    private readonly RefreshTokenCommandValidator _sut;

    public RefreshTokenQueryValidatorTests()
    {
        _sut = new RefreshTokenCommandValidator();
    }

    [Fact]
    public void RefreshTokenValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_token_string");

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RefreshTokenValidator_ShouldFail_WithInvalidToken(string? value)
    {
        // Arrange
        var command = new RefreshTokenCommand(value!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiredAccessToken");
    }
}
