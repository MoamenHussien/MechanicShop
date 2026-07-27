using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.Update;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateCustomerCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateCustomerHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        await _context.VehicleMakes.AddAsync(make);
        var customer = CustomerFactory.CreateCustomer(email: "update1@localhost.com").Value;
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
            customer.Id,
            "Updated Name",
            "updated@localhost.com",
            "+201012345678",
            [new UpdateVehicleCommand(Guid.NewGuid(), 2022, "XYZ-789", make.VehicleModels.First().Id)]
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var updatedCustomer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.vehicles)
            .SingleAsync(c => c.Id == customer.Id);

        Assert.Equal("Updated Name", updatedCustomer.Name);
        Assert.Equal("updated@localhost.com", updatedCustomer.Email);
        Assert.Equal("+201012345678", updatedCustomer.PhoneNumber);
        Assert.Single(updatedCustomer.vehicles);
    }

    [Fact]
    public async Task UpdateCustomerHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var command = new UpdateCustomerCommand(
            Guid.NewGuid(),
            "Updated Name",
            "updated@localhost.com",
            "+201012345678",
            [new UpdateVehicleCommand(Guid.NewGuid(), 2022, "XYZ-789", Guid.NewGuid())]
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.TheCustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateCustomerHandler_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var customer1 = CustomerFactory.CreateCustomer(email: "existing@localhost.com").Value;
        var customer2 = CustomerFactory.CreateCustomer(email: "update2@localhost.com").Value;
        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync(default);

        var command = new UpdateCustomerCommand(
            customer2.Id,
            "Updated Name",
            "existing@localhost.com", // Duplicate email from customer1
            "+201012345678",
            [new UpdateVehicleCommand(Guid.NewGuid(), 2022, "XYZ-789", Guid.NewGuid())]
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.CustomerWithThisEmailIsAlreadyExists.Code, result.TopError.Code);
    }
}
