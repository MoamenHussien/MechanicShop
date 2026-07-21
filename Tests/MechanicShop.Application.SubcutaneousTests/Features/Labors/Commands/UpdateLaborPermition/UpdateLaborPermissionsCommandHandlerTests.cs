using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Labors.Commands.UpdateLaborPermition;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateLaborPermissionsCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    [Fact]
    public async Task UpdateLaborPermissionsHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var registerCommand = new RegisterLaborCommand(
            email: $"labor_{Guid.NewGuid()}@test.com",
            password: "Password123!",
            FirstName: "John",
            LastName: "Doe",
            Roles: ["Labor"],
            Claims: []
        );

        var registerResult = await _mediator.Send(registerCommand);
        Assert.True(registerResult.IsSuccess);

        var command = new UpdateLaborPermissionsCommand(
            LaborId: registerResult.Value,
            Roles: ["Manager"],
            Claims: []
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateLaborPermissionsHandler_WithInvalidLaborId_ShouldFail()
    {
        // Arrange
        var command = new UpdateLaborPermissionsCommand(
            LaborId: Guid.NewGuid(),
            Roles: ["Manager"],
            Claims: []
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }
}
