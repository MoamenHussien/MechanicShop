using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _sut;

    public UpdateWorkOrderRepairTasksCommandValidatorTests()
    {
        _sut = new UpdateWorkOrderRepairTasksCommandValidator();
    }

    [Fact]
    public void UpdateWorkOrderRepairTasksCommandValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UpdateWorkOrderRepairTasksCommandValidator_ShouldFail_WithEmptyWorkOrderId()
    {
        // Arrange
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.Empty, [Guid.NewGuid()]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "WorkOrderid");
    }

    [Fact]
    public void UpdateWorkOrderRepairTasksCommandValidator_ShouldFail_WithNullRepairTasks()
    {
        // Arrange
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "RepairTasksIds");
    }

    [Fact]
    public void UpdateWorkOrderRepairTasksCommandValidator_ShouldFail_WithEmptyRepairTasks()
    {
        // Arrange
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "RepairTasksIds");
    }
}
