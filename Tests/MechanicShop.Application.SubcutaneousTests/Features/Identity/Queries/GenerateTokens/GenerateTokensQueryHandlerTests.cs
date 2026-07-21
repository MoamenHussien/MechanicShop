using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Tests.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GenerateTokens;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GenerateTokensQueryHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();

    [Fact]
    public async Task GenerateTokenHandler_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "testuser@localhost.com",
            UserName = "testuser@localhost.com",
            EmailConfirmed = true
        };
        var password = "ValidPassword123!";

        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        var command = new GenerateTokenCommand(appUser.Email!, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.True(result.Value.ExpiresOnUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateTokenHandler_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "anotheruser@localhost.com",
            UserName = "anotheruser@localhost.com",
            EmailConfirmed = true
        };
        
        var password = "ValidPassword123!";

        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        var command = new GenerateTokenCommand(appUser.Email!, "WrongPassword123!");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Invalid_Login_Attempt", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateTokenHandler_WithNonExistentEmail_ShouldFail()
    {
        // Arrange
        var command = new GenerateTokenCommand("nonexistent@localhost.com", "ValidPassword123!");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateTokenHandler_WithLockedUser_ShouldFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "lockeduser@localhost.com",
            UserName = "lockeduser@localhost.com",
            EmailConfirmed = true
        };
        
        var password = "ValidPassword123!";
        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        // Lock the user out
        await userManager.SetLockoutEnabledAsync(appUser, true);
        await userManager.SetLockoutEndDateAsync(appUser, DateTimeOffset.UtcNow.AddMinutes(15));

        var command = new GenerateTokenCommand(appUser.Email!, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("User_Locked", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateTokenHandler_WithUnconfirmedEmail_ShouldFail()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "unconfirmeduser@localhost.com",
            UserName = "unconfirmeduser@localhost.com",
            EmailConfirmed = false // Crucial missing confirmation
        };
        
        var password = "ValidPassword123!";
        var createResult = await userManager.CreateAsync(appUser, password);
        Assert.True(createResult.Succeeded);

        var command = new GenerateTokenCommand(appUser.Email!, password);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Email_Not_Confirmed", result.TopError.Code);
    }
}
