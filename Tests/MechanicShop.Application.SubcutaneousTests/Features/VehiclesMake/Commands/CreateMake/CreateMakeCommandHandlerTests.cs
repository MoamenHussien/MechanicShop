using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.CreateMake;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateMakeCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task CreateMakeHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new CreateMakeCommand(
            Make: "Honda",
            Models:
            [
                new CreateVehicleModelCommand("Civic")
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var make = await _context.VehicleMakes
            .Include(m => m.VehicleModels)
            .SingleAsync(m => m.Id == result.Value);

        Assert.Equal(command.Make.CapitalizeFirstLetter(), make.Make);

        Assert.Single(make.VehicleModels);

        var model = make.VehicleModels.Single();

        Assert.Equal(command.Models[0].model.CapitalizeFirstLetter(), model.Model);
    }
}
