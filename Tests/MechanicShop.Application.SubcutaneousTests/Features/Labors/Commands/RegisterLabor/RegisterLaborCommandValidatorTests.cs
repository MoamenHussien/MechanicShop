using System.Security.Claims;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.RegisterLabor;

public class RegisterLaborCommandValidatorTests
{
    private readonly RegisterLaborCommandValidator _sut;

    public RegisterLaborCommandValidatorTests()
    {
        _sut = new RegisterLaborCommandValidator();
    }

    private static RegisterLaborCommand CreateCommand(
        string? email = null,
        string? password = null,
        string? firstName = null,
        string? lastName = null,
        List<string>? roles = null,
        List<Claim>? claims = null)
    {
        return new(
            email ?? "labor@test.com",
            password ?? "Password123!",
            firstName ?? "John",
            lastName ?? "Doe",
            roles ?? ["Labor"],
            claims ?? [new Claim("test", "test")]);
    }

    [Fact]
    public void RegisterLaborCommandValidator_ShouldSucceed_WithValidInputs()
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
    public void RegisterLaborCommandValidator_ShouldFail_WithInvalidEmail(string? value)
    {
        // Arrange
        var command = new RegisterLaborCommand(value!, "Password123!", "John", "Doe", ["Labor"], []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("short")] // length < 8
    [InlineData(null)]
    public void RegisterLaborCommandValidator_ShouldFail_WithInvalidPassword(string? value)
    {
        // Arrange
        var command = new RegisterLaborCommand("test@test.com", value!, "John", "Doe", ["Labor"], []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("J")] // length < 2
    [InlineData(null)]
    public void RegisterLaborCommandValidator_ShouldFail_WithInvalidFirstName(string? value)
    {
        // Arrange
        var command = new RegisterLaborCommand("test@test.com", "Password123!", value!, "Doe", ["Labor"], []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "FirstName");
    }

    [Fact]
    public void RegisterLaborCommandValidator_ShouldFail_WhenFirstNameLengthGreaterThan50()
    {
        // Arrange
        var command = CreateCommand(firstName: new string('A', 55));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }

    [Fact]
    public void RegisterLaborCommandValidator_ShouldFail_WithNullClaims()
    {
        // Arrange
        var command = new RegisterLaborCommand("test@test.com", "Password123!", "John", "Doe", ["Labor"], null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Claims");
    }
}
