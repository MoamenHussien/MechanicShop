using MechanicShop.Tests.Common.Customers;
using Xunit;

public class CustomerMapperTests
{
    private static VehicleModel CreateVehicleModelWithMake()
    {
        var make = VehicleMakeFactory.CreateVehicleMake(Make: "Toyota").Value;
        var model = VehicleModelFactory.CreateVehiclModel(
            model: "Corolla",
            vehicleMake: make).Value;
        return model;
    }

    [Fact]
    public void SingleToDto_WhenCustomerIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var vehicleModel = CreateVehicleModelWithMake();

        var sourceCustomer = CustomerFactory.CreateCustomer(
            Guid.NewGuid(),
            "Moamen",
            "+201014245762",
            "MoamenHussien25@gmail.com",
            [VehicleFactory.CreateVehicle(vehicleModel: vehicleModel).Value]).Value;

        // Act
        var customerDto = sourceCustomer.ToDto();

        // Assert
        Assert.Equal(sourceCustomer.Id, customerDto.CustomerId);
        Assert.Equal(sourceCustomer.Name, customerDto.Name);
        Assert.Equal(sourceCustomer.PhoneNumber, customerDto.PhoneNumber);
        Assert.Equal(sourceCustomer.Email, customerDto.Email);

        var sourceVehicle = Assert.Single(sourceCustomer.vehicles);
        var mappedVehicle = Assert.Single(customerDto.Vehicles);

        Assert.Equal(sourceVehicle.Id, mappedVehicle.Id);
    }

    [Fact]
    public void GroupToDto_WhenCustomersAreValid_ShouldMapAllCustomers()
    {
        // Arrange
        var vehicleModel = CreateVehicleModelWithMake();

        var firstCustomer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModel: vehicleModel).Value]).Value;
        var secondCustomer = CustomerFactory.CreateCustomer(
            vehicles: [VehicleFactory.CreateVehicle(vehicleModel: vehicleModel).Value]).Value;

        List<Customer> sourceCustomers =
        [
            firstCustomer,
            secondCustomer
        ];

        // Act
        var customerDtos = sourceCustomers.ToDto();

        // Assert
        Assert.Equal(sourceCustomers.Count, customerDtos.Count);

        Assert.Contains(customerDtos, dto => dto.CustomerId == firstCustomer.Id);
        Assert.Contains(customerDtos, dto => dto.CustomerId == secondCustomer.Id);
    }
}