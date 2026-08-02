using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskPartCommandValidatorTests
{
    private readonly CreatePartCommandValidator _sut;

    public CreateRepairTaskPartCommandValidatorTests()
    {
        _sut = new CreatePartCommandValidator();
    }

    private static CreateRepairTaskPartCommand CreateCommand(
        string? name = null,
        decimal? cost = null,
        int? quantity = null)
    {
        return new CreateRepairTaskPartCommand(
            name ?? "Valid Part Name",
            cost ?? 100m,
            quantity ?? 1);
    }

    [Fact]
    public void CreateRepairTaskPartValidator_ShouldSucceed_WithValidInputs()
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
    public void CreateRepairTaskPartValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new CreateRepairTaskPartCommand(value!, 100m, 1);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Fact]
    public void CreateRepairTaskPartValidator_ShouldFail_WhenNameExceedsMaximumLength()
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
    public void CreateRepairTaskPartValidator_ShouldFail_WithInvalidCost(decimal value)
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
    public void CreateRepairTaskPartValidator_ShouldFail_WithInvalidQuantity(int value)
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
