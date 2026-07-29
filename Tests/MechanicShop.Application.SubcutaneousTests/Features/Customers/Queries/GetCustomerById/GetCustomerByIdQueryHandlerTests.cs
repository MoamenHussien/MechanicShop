using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomerByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetCustomerByIdHandler_WithValidId_ShouldSucceed()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer(email: "getcustomer@localhost.com").Value;
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync(default);

        var query = new GetCustomerByIdQuery(customer.Id);

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(customer.Id, result.Value.CustomerId);
        Assert.Equal(customer.Name, result.Value.Name);
        Assert.Equal(customer.Email, result.Value.Email);
        Assert.Equal(customer.PhoneNumber, result.Value.PhoneNumber);
    }

    [Fact]
    public async Task GetCustomerByIdHandler_WithInvalidId_ShouldFail()
    {
        // Arrange
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(query);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.TheCustomerNotFound.Code, result.TopError.Code);
    }
}
