using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateCustomerCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task CreateCustomerHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();

        var command = new CreateCustomerCommand(
            name: "Moamen",
            email: "moamen@test.com",
            PhoneNumber: "+201014245762",
            Vehicles:
            [
                new CreateVehicleCommand(
                    2020,
                    "ABC123",
                    vehicleModel.Id)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var customer = await _context.Customers
            .Include(c => c.vehicles)
            .SingleAsync(c => c.Id == result.Value.CustomerId);

        Assert.Equal(command.name.CapitalizeFirstLetter(), customer.Name);
        Assert.Equal(command.email.Trim().ToLowerInvariant(), customer.Email);
        Assert.Equal(command.PhoneNumber, customer.PhoneNumber);

        Assert.Single(customer.vehicles);

        var vehicle = customer.vehicles.Single();

        Assert.Equal(command.Vehicles[0].LicensePlate, vehicle.LicensePlate);
        Assert.Equal(command.Vehicles[0].VehicleModelId, vehicle.VehicleModelId);
    }

    [Fact]
    public async Task CreateCustomerHandler_WithExistingEmail_ShouldFail()
    {
        // Arrange
        var vehicleModel = await _context.VehicleModels.FirstAsync();

        var customer = CustomerFactory.CreateCustomer(
            email: "existing@test.com",
            vehicles:
            [
                VehicleFactory.CreateVehicle(vehicleModelId: vehicleModel.Id).Value
            ]).Value;

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var command = new CreateCustomerCommand(
            name: "Moamen",
            email: "existing@test.com",
            PhoneNumber: "+201014245762",
            Vehicles:
            [
                new CreateVehicleCommand(
                    2020,
                    "XYZ123",
                    vehicleModel.Id)
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.CustomerWithThisEmailIsAlreadyExists.Code, result.TopError.Code);
    }

    [Fact]
    public async Task CreateCustomerHandler_WithInvalidVehicleModel_ShouldFail()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            name: "Moamen",
            email: "moamen@test.com",
            PhoneNumber: "+201014245762",
            Vehicles:
            [
                new CreateVehicleCommand(
                    2020,
                    "ABC123",
                    Guid.NewGuid())
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }
}