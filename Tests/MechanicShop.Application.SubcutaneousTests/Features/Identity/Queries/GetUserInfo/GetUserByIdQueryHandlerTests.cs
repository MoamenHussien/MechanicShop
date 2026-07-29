using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfo;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetUserByIdQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task GetUserByIdHandler_WithValidData_ShouldSucceed()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "getuser@localhost.com",
            UserName = "getuser@localhost.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(appUser, "ValidPassword123!");
        Assert.True(createResult.Succeeded);

        var command = new GetUserByIdCommand(appUser.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(appUser.Id, result.Value.UserId);
        Assert.Equal(appUser.Email, result.Value.Email);
    }

    [Fact]
    public async Task GetUserByIdHandler_WithInvalidUserId_ShouldFail()
    {
        // Arrange
        var command = new GetUserByIdCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }
}
