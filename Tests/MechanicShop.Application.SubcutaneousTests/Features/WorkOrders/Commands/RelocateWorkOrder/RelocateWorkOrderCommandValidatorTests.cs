using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidatorTests
{
    private readonly RelocateWorkOrderCommandValidator _sut;

    public RelocateWorkOrderCommandValidatorTests()
    {
        _sut = new RelocateWorkOrderCommandValidator();
    }

    [Fact]
    public void RelocateWorkOrderCommandValidator_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            Spot.A);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RelocateWorkOrderCommandValidator_ShouldFail_WithEmptyWorkOrderId()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.Empty,
            DateTimeOffset.UtcNow.AddDays(1),
            Spot.A);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void RelocateWorkOrderCommandValidator_ShouldFail_WithPastDate()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-1),
            Spot.A);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "NewStartDateTimeUtc");
    }

    [Fact]
    public void RelocateWorkOrderCommandValidator_ShouldFail_WithInvalidSpot()
    {
        // Arrange
        var command = new RelocateWorkOrderCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            (Spot)999);

        // Act
        var result = _sut.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, n => n.PropertyName == "NewSpot");
    }
}