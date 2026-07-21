using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomersQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetCustomersHandler_WithExistingCustomers_ShouldSucceed()
    {
        // Arrange
        var make = VehicleMakeFactory.CreateVehicleMake().Value;
        await _context.VehicleMakes.AddAsync(make);

        var customer1 = CustomerFactory.CreateCustomer(email: "list1@localhost.com", vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;
        var customer2 = CustomerFactory.CreateCustomer(email: "list2@localhost.com", vehicles: [VehicleFactory.CreateVehicle(vehicleModelId: make.VehicleModels.First().Id).Value]).Value;

        await _context.Customers.AddRangeAsync(customer1, customer2);
        await _context.SaveChangesAsync(default);

        var query = new GetCustomersQuery();

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Contains(result.Value, c => c.CustomerId == customer1.Id);
        Assert.Contains(result.Value, c => c.CustomerId == customer2.Id);
    }

    [Fact]
    public async Task GetCustomersHandler_WithEmptyDatabase_ShouldFail()
    {
        // Arrange
        // Ensure the database is completely empty of customers
        _context.Invoices.RemoveRange(await _context.Invoices.ToListAsync());
        _context.WorkOrders.RemoveRange(await _context.WorkOrders.ToListAsync());
        _context.Vehicles.RemoveRange(await _context.Vehicles.ToListAsync());
        _context.Customers.RemoveRange(await _context.Customers.ToListAsync());
        await _context.SaveChangesAsync(default);

        var cache = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(factory.Services);
        await cache.RemoveAsync("AllCustomers", default);

        var query = new GetCustomersQuery();

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.NotFoundAnyCustomers.Code, result.TopError.Code);
    }
}