using MechanicShop.Tests.Common.Customers;
using Xunit;

public class CustomerMapperTests
{
    [Fact]
    public void SingleToDto_WhenCustomerIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var sourceCustomer = CustomerFactory.CreateCustomer(
            Guid.NewGuid(),
            "Moamen",
            "+201014245762",
            "MoamenHussien25@gmail.com",
            [VehicleFactory.CreateVehicle().Value]).Value;

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
        var firstCustomer = CustomerFactory.CreateCustomer().Value;
        var secondCustomer = CustomerFactory.CreateCustomer().Value;

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