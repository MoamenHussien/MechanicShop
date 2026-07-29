using MechanicShop.Tests.Common.Customers;
using Xunit;

public class CustomerTests
{
    [Fact]
    public void CreateCustomer_ShouldSucceed_WithValidData()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = "Customer #1";
        const string phone = "5555555555";
        const string email = "customer01@localhost";
        List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value];

        // Act
        var result = CustomerFactory.CreateCustomer(
            id: id,
            name: name,
            email: email,
            phoneNumber: phone,
            vehicles: vehicles);

        // Assert
        Assert.True(result.IsSuccess);

        var customer = result.Value;

        Assert.Equal(id, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(phone, customer.PhoneNumber);
        Assert.Equal(email, customer.Email);
        Assert.Single(customer.vehicles);
        Assert.Equal(vehicles[0].Id, customer.vehicles.First().Id);
    }

    [Fact]
    public void CreateCustomer_ShouldSucceed_WithEmptyId()
    {
        // Act
        // var result = CustomerFactory.CreateCustomer(id: Guid.Empty);

        var result = Customer.Create(
            id: Guid.Empty,
            name: "Customer #1",
            email: "customer01@localhost",
            phone: "5555555555",
            vehicles: [VehicleFactory.CreateVehicle().Value, VehicleFactory.CreateVehicle().Value]);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WithInvalidName(string value)
    {
        // Act
        var result = CustomerFactory.CreateCustomer(name: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WithInvalidEmail(string value)
    {
        // Act
        var result = CustomerFactory.CreateCustomer(email: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WithInvalidPhone(string value)
    {
        // Act
        var result = CustomerFactory.CreateCustomer(phoneNumber: value);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.PhoneRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateCustomer_ShouldFail_WithInvalidVehicles()
    {
        // Act
        // var result = CustomerFactory.CreateCustomer(vehicles: null);
        var result = Customer.Create(
            id: Guid.NewGuid(),
            name: "Customer #1",
            email: "customer01@localhost",
            phone: "5555555555",
            vehicles: null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.VehiclesRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateCustomer_ShouldFail_WithEmptyVehicles()
    {
        // Act
        var result = CustomerFactory.CreateCustomer(vehicles: []);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.VehiclesRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateCustomer_ShouldSucceed_WithValidData()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.Update(
            "Updated Name",
            "updated@email.com",
            "123456789");

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("Updated Name", customer.Name);
        Assert.Equal("updated@email.com", customer.Email);
        Assert.Equal("123456789", customer.PhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    public void UpdateCustomer_ShouldFail_WithInvalidName(string value)
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.Update(
            value!,
            "email@test.com",
            "123456789");

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateCustomer_ShouldFail_WithInvalidEmail(string? value)
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.Update(
            "Customer",
            value!,
            "123456789");

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateCustomer_ShouldFail_WithInvalidPhone(string? value)
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.Update(
            "Customer",
            "email@test.com",
            value!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CustomerErrors.PhoneRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpSertVehicles_ShouldSucceed_WithValidData()
    {
        // Arrange
        var originalVehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(
            vehicles: [originalVehicle]).Value;

        var updatedVehicle = VehicleFactory.CreateVehicle(
            id: originalVehicle.Id,
            year: 2025).Value;

        var newVehicle = VehicleFactory.CreateVehicle().Value;

        // Act
        var result = customer.UpSertVehicles(
            [updatedVehicle, newVehicle]);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, customer.vehicles.Count);

        Assert.Contains(customer.vehicles,
            n => n.Id == updatedVehicle.Id && n.Year == 2025);

        Assert.Contains(customer.vehicles,
            n => n.Id == newVehicle.Id);
    }

    [Fact]
    public void UpSertVehicles_ShouldFail_WithInvalidVehicles()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.UpSertVehicles(null!);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            CustomerErrors.VehiclesRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpSertVehicles_ShouldFail_WithEmptyVehicles()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;

        // Act
        var result = customer.UpSertVehicles([]);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            CustomerErrors.VehiclesRequired.Code,
            result.TopError.Code);
    }

    [Fact]
    public void UpSertVehicles_ShouldRemoveVehiclesNotIncluded()
    {
        // Arrange
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [vehicle1, vehicle2]).Value;

        var incoming = VehicleFactory.CreateVehicle(
            id: vehicle2.Id).Value;

        // Act
        var result = customer.UpSertVehicles([incoming]);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        Assert.Single(customer.vehicles);
        Assert.Equal(vehicle2.Id, customer.vehicles.Single().Id);
    }
}
