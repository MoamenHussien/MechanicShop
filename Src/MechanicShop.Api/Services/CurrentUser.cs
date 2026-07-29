using System.Security.Claims;

public class CurrentUser(IHttpContextAccessor contextAccessor) : IUser
{
    public Guid? Id
    {
        get
        {
            var result = contextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier)
                .ToGuid();

            return result is { IsSuccess: true }
                ? result.Value
                : null;
        }
    }
}
