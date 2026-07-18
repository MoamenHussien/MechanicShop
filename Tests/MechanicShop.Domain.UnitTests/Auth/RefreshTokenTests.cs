using MechanicShop.Tests.Common.Auth;
using Xunit;

public class RefreshTokenTests
{
   [Fact]
   public void CreateRefreshToken_ShouldSucceed_WhenEnterValidInfo()
   {
      // Arrange
      Guid Id = Guid.NewGuid();
      const string tokenValue = "Token";
      Guid UserId = Guid.NewGuid();
      var ExpiresOnUtc = DateTimeOffset.UtcNow.AddDays(7);

      // Act
      var result = RefreshTokenFactory.CreateRefreshToken(Id, tokenValue, UserId, ExpiresOnUtc);

      // Assert
      Assert.True(result.IsSuccess);
      var Token = result.Value;
      Assert.NotNull(tokenValue);
      Assert.Equal(Id, Token.Id);
      Assert.False(string.IsNullOrWhiteSpace(Token.Token));
      Assert.Equal(tokenValue, Token.Token);
      Assert.Equal(UserId, Token.UserId);
      Assert.True(ExpiresOnUtc > DateTimeOffset.UtcNow);
      Assert.Equal(ExpiresOnUtc, Token.ExpiresOnUtc);
   }

   [Fact]
   public void CreateRefreshToken_ShouldGenerateId_WhenIdIsEmpty()
   {
      var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

      Assert.True(result.IsSuccess);
      Assert.NotEqual(Guid.Empty, result.Value.Id);
   }

   [Theory]
   [InlineData("")]
   [InlineData(" ")]
   [InlineData("   ")]
   [InlineData(null)]
   public void CreateRefreshToken_ShouldFail_WhenTokenIsInvalid(string? tokenValue)
   {
      // Act
      var result = RefreshTokenFactory.CreateRefreshToken(token: tokenValue);
      // Assert
      Assert.True(result.IsError);
      Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
   }

   [Fact]
   public void CreateRefreshToken_ShouldFail_WhenEnterInvalidUserId()
   {
      // Act
      var result = RefreshTokenFactory.CreateRefreshToken(userId: Guid.Empty);
      // Assert
      Assert.True(result.IsError);
      Assert.Equal(RefreshTokenErrors.UserIdRequired.Code, result.TopError.Code);
   }

   [Fact]
   public void CreateRefreshToken_ShouldFail_WhenExpiresOnUtcIsInPast()
   {
      // Arrage 
      var TheDayBefor = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));
      // Act
      var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: TheDayBefor);
      // Assert
      Assert.True(result.IsError);
      Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
   }
}


