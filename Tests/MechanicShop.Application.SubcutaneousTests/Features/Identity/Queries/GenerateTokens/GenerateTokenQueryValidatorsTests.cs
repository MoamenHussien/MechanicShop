using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryValidatorsTests
{
    private readonly GenerateTokenCommandValidator _sut;

    public GenerateTokenQueryValidatorsTests()
    {
        _sut = new GenerateTokenCommandValidator();
    }

    private static GenerateTokenCommand CreateCommand(
        string? email = null,
        string? password = null)
    {
        return new GenerateTokenCommand(
            email ?? "test@example.com",
            password ?? "Password123!"
        );
    }

    [Fact]
    public void GenerateTokenValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("invalid-email")]
    [InlineData(null)]
    public void GenerateTokenValidator_ShouldFail_WithInvalidEmail(string? value)
    {
        // Arrange
        var command = new GenerateTokenCommand(value!, "Password123!");

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateTokenValidator_ShouldFail_WithEmptyPassword(string? value)
    {
        // Arrange
        var command = new GenerateTokenCommand("test@example.com", value!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "password");
    }

    [Theory]
    [InlineData("short")] // Less than 8 characters
    [InlineData("thispasswordiswaytoolongtoobeacceptedbythevalidatorbecauseitexceeds30chars")] // More than 30
    public void GenerateTokenValidator_ShouldFail_WithInvalidPasswordLength(string value)
    {
        // Arrange
        var command = CreateCommand(password: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "password");
    }
}
