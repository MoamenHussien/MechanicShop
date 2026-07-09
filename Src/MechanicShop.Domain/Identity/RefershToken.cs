using System.Runtime.CompilerServices;
using System.Security.AccessControl;

public sealed class RefreshToken : AuditableEntity
{
    public Guid UserId { get;private set; } 
    public string Token { get; private set; }
    public DateTimeOffset ExpiresOnUtc { get; private set; }
    public DateTimeOffset? RevokedOn { get;private set; } 

#pragma warning disable CS8618
    private RefreshToken()
{
    
}
#pragma warning restore CS8618

private RefreshToken(Guid id ,Guid Userid , string Token,DateTimeOffset ExpirationOn):base(id)
{
    this.UserId = Userid;
    this.Token = Token;
    this.ExpiresOnUtc =ExpirationOn;
    RevokedOn=null;
}
    public bool IsActive()
    {
        return ExpiresOnUtc > DateTimeOffset.UtcNow && RevokedOn is null;
    }

    public bool IsExpired()
    {
        return !IsActive();
    }

    public static Result<RefreshToken> Create(Guid id ,string token,Guid userid,DateTimeOffset ExpirationOnUtc)
    {
        if (id == Guid.Empty)
        {
           id =Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if(userid == Guid.Empty)
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if(ExpirationOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(id,userid,token,ExpirationOnUtc);
    }
}