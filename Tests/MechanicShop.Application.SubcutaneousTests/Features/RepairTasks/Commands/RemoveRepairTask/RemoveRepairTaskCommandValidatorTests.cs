using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

public class RemoveRepairTaskCommandValidatorTests
{
    private readonly DeleteRepairTaskCommandValidator _sut;

    public RemoveRepairTaskCommandValidatorTests()
    {
        _sut = new DeleteRepairTaskCommandValidator();
    }

    [Fact]
    public void RemoveRepairTaskValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new DeleteRepairTaskCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RemoveRepairTaskValidator_ShouldFail_WithEmptyId()
    {
        // Arrange
        var command = new DeleteRepairTaskCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "id");
    }
}
