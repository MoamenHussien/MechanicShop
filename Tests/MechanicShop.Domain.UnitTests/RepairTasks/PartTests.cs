using MechanicShop.Tests.Common.RepaireTasks;
using Xunit;

public class PartTests
{
    [Fact]
    public void CreatePart_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        const decimal costs = 100m;
        const string name = "Brake Pad";
        const int quantity = 2;

        // Act
        var result = PartFactory.CreatePart(
            id: id,
            cost: costs,
            name: name,
            quantity: quantity);

        // Assert
        Assert.True(result.IsSuccess);

        var part = result.Value;

        Assert.Equal(id, part.Id);
        Assert.Equal(costs, part.Costs);
        Assert.Equal(name, part.Name);
        Assert.Equal(quantity, part.Quantity);
        Assert.Equal(200m, part.PartFinalCosts);
    }

    [Fact]
    public void CreatePart_ShouldSucceed_WithEmptyId()
    {
        // Act
        var result = Part.Create(id : Guid.Empty,Costs: 100,Name : "Brake Pad",Quantity : 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreatePart_ShouldFail_WithInvalidCosts(decimal value)
    {
        // Act
        var result = PartFactory.CreatePart(cost: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.partCostLowerThanZero.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreatePart_ShouldFail_WithInvalidName(string? value)
    {
        // Act
        var result = Part.Create(id : Guid.Empty,Costs: 100,Name : value!,Quantity : 2);


        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.ValidPartName.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreatePart_ShouldFail_WithInvalidQuantity(int value)
    {
        // Act
        var result = PartFactory.CreatePart(quantity: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.PartQuantityLowerThanZero.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdatePart_ShouldSucceed_WithValidData()
    {
        // Arrange
        var part = PartFactory.CreatePart().Value;

        // Act
        var result = part.Update(
            Costs: 200m,
            Name: "Brake Disc",
            Quantity: 3);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(200m, part.Costs);
        Assert.Equal("Brake Disc", part.Name);
        Assert.Equal(3, part.Quantity);
        Assert.Equal(600m, part.PartFinalCosts);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdatePart_ShouldFail_WithInvalidCosts(decimal value)
    {
        // Arrange
        var part = PartFactory.CreatePart().Value;

        // Act
        var result = part.Update(
            Costs: value,
            Name: "Brake Disc",
            Quantity: 3);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.partCostLowerThanZero.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdatePart_ShouldFail_WithInvalidName(string? value)
    {
        // Arrange
        var part = PartFactory.CreatePart().Value;

        // Act
        var result = part.Update(
            Costs: 200m,
            Name: value!,
            Quantity: 3);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.ValidPartName.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdatePart_ShouldFail_WithInvalidQuantity(int value)
    {
        // Arrange
        var part = PartFactory.CreatePart().Value;

        // Act
        var result = part.Update(
            Costs: 200m,
            Name: "Brake Disc",
            Quantity: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(PartsErrors.PartQuantityLowerThanZero.Code, result.TopError.Code);
    }
}