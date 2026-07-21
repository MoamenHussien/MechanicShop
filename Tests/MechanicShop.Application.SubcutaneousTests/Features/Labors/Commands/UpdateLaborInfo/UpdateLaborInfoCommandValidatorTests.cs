using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.UpdateLaborInfo;

public class UpdateLaborInfoCommandValidatorTests
{
    private readonly UpdateLaborInfoCommandValidator _sut;

    public UpdateLaborInfoCommandValidatorTests()
    {
        _sut = new UpdateLaborInfoCommandValidator();
    }

    private static UpdateLaborInfoCommand CreateCommand(
        Guid? id = null,
        string? firstName = null,
        string? lastName = null,
        bool? isActive = null)
    {
        return new(
            id ?? Guid.NewGuid(),
            firstName ?? "John",
            lastName ?? "Doe",
            isActive ?? true);
    }

    [Fact]
    public void UpdateLaborInfoCommandValidator_ShouldSucceed_WithValidInputs()
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
    public void UpdateLaborInfoCommandValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = CreateCommand(id: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("J")] // length < 2
    [InlineData(null)]
    public void UpdateLaborInfoCommandValidator_ShouldFail_WithInvalidFirstName(string? value)
    {
        // Arrange
        var command = new UpdateLaborInfoCommand(Guid.NewGuid(), value!, "Doe", true);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "FirstName");
    }

    [Fact]
    public void UpdateLaborInfoCommandValidator_ShouldFail_WhenFirstNameLengthGreaterThan50()
    {
        // Arrange
        var command = CreateCommand(firstName: new string('A', 55));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("D")] // length < 2
    [InlineData(null)]
    public void UpdateLaborInfoCommandValidator_ShouldFail_WithInvalidLastName(string? value)
    {
        // Arrange
        var command = new UpdateLaborInfoCommand(Guid.NewGuid(), "John", value!, true);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "LastName");
    }

    [Fact]
    public void UpdateLaborInfoCommandValidator_ShouldFail_WhenLastNameLengthGreaterThan50()
    {
        // Arrange
        var command = CreateCommand(lastName: new string('A', 55));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LastName");
    }
}
