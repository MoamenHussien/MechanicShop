using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskPartCommandValidatorTests
{
    private readonly UpdatePartCommandValidator _sut;

    public UpdateRepairTaskPartCommandValidatorTests()
    {
        _sut = new UpdatePartCommandValidator();
    }

    private static UpdatePartCommand CreateCommand(
        Guid? id = null,
        string? name = null,
        decimal? cost = null,
        int? quantity = null)
    {
        return new UpdatePartCommand(
            id,
            name ?? "Valid Part",
            cost ?? 50m,
            quantity ?? 2
        );
    }

    [Fact]
    public void UpdatePartValidator_ShouldSucceed_WithValidInputs()
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
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdatePartValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new UpdatePartCommand(Guid.NewGuid(), value!, 100m, 1);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Fact]
    public void UpdatePartValidator_ShouldFail_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var command = CreateCommand(name: new string('A', 51));

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void UpdatePartValidator_ShouldFail_WithInvalidCost(decimal value)
    {
        // Arrange
        var command = CreateCommand(cost: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "cost");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdatePartValidator_ShouldFail_WithInvalidQuantity(int value)
    {
        // Arrange
        var command = CreateCommand(quantity: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }
}