using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _sut;

    public CreateRepairTaskCommandValidatorTests()
    {
        _sut = new CreateRepairTaskCommandValidator();
    }

    private static CreateRepairTaskCommand CreateCommand(
        string? name = null,
        decimal? laborCost = null,
        RepairDurationInMinutes? duration = null,
        List<CreateRepairTaskPartCommand>? parts = null)
    {
        return new CreateRepairTaskCommand(
            name ?? "Valid Repair Task",
            laborCost ?? 150m,
            duration ?? RepairDurationInMinutes.Min60,
            parts ?? [new CreateRepairTaskPartCommand("Part 1", 50m, 2)]);
    }

    [Fact]
    public void CreateRepairTaskValidator_ShouldSucceed_WithValidInputs()
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
    public void CreateRepairTaskValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new CreateRepairTaskCommand(value!, 150m, RepairDurationInMinutes.Min60, [new CreateRepairTaskPartCommand("Part 1", 50m, 2)]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Fact]
    public void CreateRepairTaskValidator_ShouldFail_WhenNameExceedsMaximumLength()
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
    public void CreateRepairTaskValidator_ShouldFail_WithInvalidLaborCost(decimal value)
    {
        // Arrange
        var command = CreateCommand(laborCost: value);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LaborCost");
    }

    [Fact]
    public void CreateRepairTaskValidator_ShouldFail_WithInvalidDuration()
    {
        // Arrange
        var command = CreateCommand(duration: (RepairDurationInMinutes)999);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "duration");
    }

    [Fact]
    public void CreateRepairTaskValidator_ShouldFail_WithNullParts()
    {
        // Arrange
        var command = new CreateRepairTaskCommand("Valid Repair Task", 150m, RepairDurationInMinutes.Min60, null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Parts");
    }

    [Fact]
    public void CreateRepairTaskValidator_ShouldFail_WithEmptyParts()
    {
        // Arrange
        var command = CreateCommand(parts: []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Parts");
    }
}
