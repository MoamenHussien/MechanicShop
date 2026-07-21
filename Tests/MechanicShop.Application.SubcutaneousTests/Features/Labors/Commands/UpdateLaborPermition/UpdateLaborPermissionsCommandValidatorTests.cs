using System.Security.Claims;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.UpdateLaborPermition;

public class UpdateLaborPermissionsCommandValidatorTests
{
    private readonly UpdateLaborPermissionsCommandValidator _sut;

    public UpdateLaborPermissionsCommandValidatorTests()
    {
        _sut = new UpdateLaborPermissionsCommandValidator();
    }

    private static UpdateLaborPermissionsCommand CreateCommand(
        Guid? laborId = null,
        List<string>? roles = null,
        List<Claim>? claims = null)
    {
        return new(
            laborId ?? Guid.NewGuid(),
            roles ?? ["Labor"],
            claims ?? [new Claim("test", "test")]);
    }

    [Fact]
    public void UpdateLaborPermissionsCommandValidator_ShouldSucceed_WithValidInputs()
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
    public void UpdateLaborPermissionsCommandValidator_ShouldFail_WithEmptyLaborId()
    {
        // Arrange
        var command = CreateCommand(laborId: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "LaborId");
    }

    [Fact]
    public void UpdateLaborPermissionsCommandValidator_ShouldFail_WithEmptyRoles()
    {
        // Arrange
        var command = CreateCommand(roles: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Roles");
    }

    [Fact]
    public void UpdateLaborPermissionsCommandValidator_ShouldFail_WithNullRoles()
    {
        // Arrange
        var command = new UpdateLaborPermissionsCommand(Guid.NewGuid(), null!, []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Roles");
    }

    [Fact]
    public void UpdateLaborPermissionsCommandValidator_ShouldFail_WithNullClaims()
    {
        // Arrange
        var command = new UpdateLaborPermissionsCommand(Guid.NewGuid(), ["Labor"], null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "Claims");
    }
}
