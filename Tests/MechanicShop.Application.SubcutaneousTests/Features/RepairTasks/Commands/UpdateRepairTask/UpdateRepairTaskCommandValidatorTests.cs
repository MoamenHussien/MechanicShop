using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _sut;

    public UpdateRepairTaskCommandValidatorTests()
    {
        _sut = new UpdateRepairTaskCommandValidator();
    }

    private static UpdateRepairTaskCommand CreateCommand(
        Guid? id = null,
        string? name = null,
        decimal? laborCost = null,
        RepairDurationInMinutes? duration = null,
        List<UpdatePartCommand>? parts = null)
    {
        return new UpdateRepairTaskCommand(
            id ?? Guid.NewGuid(),
            name ?? "Valid Name",
            laborCost ?? 100m,
            duration ?? RepairDurationInMinutes.Min30,
            parts ?? [new UpdatePartCommand(Guid.NewGuid(), "Valid Part", 50m, 1)]);
    }

    [Fact]
    public void UpdateRepairTaskValidator_ShouldSucceed_WithValidInputs()
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
    public void UpdateRepairTaskValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = CreateCommand(id: Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateRepairTaskValidator_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var command = new UpdateRepairTaskCommand(Guid.NewGuid(), value!, 100m, RepairDurationInMinutes.Min30, [new UpdatePartCommand(Guid.NewGuid(), "Valid Part", 50m, 1)]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
    }

    [Fact]
    public void UpdateRepairTaskValidator_ShouldFail_WhenNameExceedsMaximumLength()
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
    public void UpdateRepairTaskValidator_ShouldFail_WithInvalidLaborCost(decimal value)
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
    public void UpdateRepairTaskValidator_ShouldFail_WithInvalidDuration()
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
    public void UpdateRepairTaskValidator_ShouldFail_WithNullParts()
    {
        // Arrange
        var command = new UpdateRepairTaskCommand(Guid.NewGuid(), "Valid Name", 100m, RepairDurationInMinutes.Min30, null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Parts");
    }

    [Fact]
    public void UpdateRepairTaskValidator_ShouldFail_WithEmptyParts()
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
