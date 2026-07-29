using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.ReAssignLabor;

public class ReAssignLaborCommandValidatorTests
{
    private readonly ReAssignLaborCommandValidator _sut;

    public ReAssignLaborCommandValidatorTests()
    {
        _sut = new ReAssignLaborCommandValidator();
    }

    [Fact]
    public void ReAssignLaborCommandValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new ReAssignLaborCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ReAssignLaborCommandValidator_ShouldFail_WithEmptyWorkOrderId()
    {
        // Arrange
        var command = new ReAssignLaborCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void ReAssignLaborCommandValidator_ShouldFail_WithEmptyLaborId()
    {
        // Arrange
        var command = new ReAssignLaborCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "LaborId");
    }
}
