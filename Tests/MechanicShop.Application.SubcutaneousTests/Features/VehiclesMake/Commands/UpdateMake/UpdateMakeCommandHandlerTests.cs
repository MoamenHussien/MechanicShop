using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.VehiclesMake.Commands.UpdateMake;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateMakeCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task UpdateMakeHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var make = VehicleMakeFactory.CreateVehicleMake().Value;

        await _context.VehicleMakes.AddAsync(make);
        await _context.SaveChangesAsync(default);
        ((DbContext)_context).ChangeTracker.Clear();

        var command = new UpdateMakeCommand(
            id: make.Id,
            Make: "Updated Toyota",
            Models:
            [
                new UpdateModelCommand(
                    ModelId: null,
                    model: "New Corolla")
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var updatedMake = await _context.VehicleMakes
            .Include(m => m.VehicleModels)
            .SingleAsync(m => m.Id == make.Id);

        Assert.Equal(command.Make.CapitalizeFirstLetter(), updatedMake.Make);

        Assert.Equal(2, updatedMake.VehicleModels.Count);
        var model = updatedMake.VehicleModels.FirstOrDefault(m => m.Model == command.Models[0].model.CapitalizeFirstLetter());

        Assert.NotNull(model);
    }

    [Fact]
    public async Task UpdateMakeHandler_WithInvalidMakeId_ShouldFail()
    {
        // Arrange
        var command = new UpdateMakeCommand(
            id: Guid.NewGuid(),
            Make: "Updated Toyota",
            Models:
            [
                new UpdateModelCommand(null, "New Corolla")
            ]);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.MakeNotFound.Code, result.TopError.Code);
    }
}
