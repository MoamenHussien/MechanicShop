using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.RegisterLabor;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RegisterLaborCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task RegisterLaborHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new RegisterLaborCommand(
            email: "newlabor@test.com",
            password: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            Roles: ["Labor"],
            Claims: []
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var employee = await _context.Employees
            .SingleAsync(e => e.Id == result.Value);

        Assert.Equal(command.FirstName.CapitalizeFirstLetter(), employee.FirstName);
        Assert.Equal(command.LastName.CapitalizeFirstLetter(), employee.LastName);
    }
}
