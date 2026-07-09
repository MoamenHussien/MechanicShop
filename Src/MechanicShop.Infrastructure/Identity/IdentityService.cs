using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class IdentityService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> user) : IIdentityService
{
    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var userinfo = await user.FindByEmailAsync(email);

        if (userinfo is null)
        {
            return Error.NotFound("User_Not_Found", $"User with email {UtilityService.MaskEmail(email)} not found");
        }

        if (await user.IsLockedOutAsync(userinfo))
        {
            return Error.Conflict("User_Locked", "User account is locked");
        }

        if (!userinfo.EmailConfirmed)
        {
            return Error.Conflict("Email_Not_Confirmed", $"email '{UtilityService.MaskEmail(email)}' not confirmed");
        }

        if (!await user.CheckPasswordAsync(userinfo, password))
        {
            return Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");
        }

        return new AppUserDto(userinfo.Id, userinfo.Email!, await user.GetRolesAsync(userinfo), await user.GetClaimsAsync(userinfo));
    }

    public async Task<Result<Guid>> CreateUserAsync(string email, string password, IList<string> roles, IList<Claim> claims)
    {
        var userinfo = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await user.CreateAsync(userinfo, password);
        if (!result.Succeeded)
        {
            return Error.Conflict("User_Creation_Failed", string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        var addClaimsResult = await user.AddClaimsAsync(userinfo, claims);

        if (!addClaimsResult.Succeeded)
        {
            await user.DeleteAsync(userinfo);
            return Error.Conflict("Add_Claims_Failed", string.Join(", ", addClaimsResult.Errors.Select(x => x.Description)));
        }
        var addRolesResult = await user.AddToRolesAsync(userinfo, roles);

        if (!addRolesResult.Succeeded)
        {
            await user.DeleteAsync(userinfo);
            return Error.Conflict("Add_Roles_Failed", string.Join(", ", addRolesResult.Errors.Select(x => x.Description)));
        }
        return userinfo.Id;
    }

    public async Task<Result<Success>> DeleteUserAsync(Guid userid)
    {
        var result = await user.FindByIdAsync(userid.ToString());
        if (result is null)
        {
            return Error.NotFound("User_Not_Found", $"User with id {userid} not found");
        }
        var deleteResult = await user.DeleteAsync(result);

        if (!deleteResult.Succeeded)
        {
            return Error.Conflict("Delete_User_Failed",string.Join(", ", deleteResult.Errors.Select(x => x.Description)));
        }

        return Result.Success;
    }

    public async Task<Result<HashSet<Guid>>> GetIdsOfUsersByRoleTypeAsync(Role role)
    {
        var users = await user.GetUsersInRoleAsync(role.ToString());
        var ids = users.Select(n => n.Id).ToHashSet();
        return ids;
    }

    public Result<string?> GetRefreshTokenFromCookies()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Error.NotFound("Cookies_Not_Found", "Cookies Not Found");
        }

        var refreshToken = httpContext.Request.Cookies["RefreshToken"];
        return refreshToken;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(Guid userId)
    {
        var result = await user.FindByIdAsync(userId.ToString());

        if (result is null)
        {
            return Error.NotFound("User_Not_Found", $"User with id {userId} not found");
        }

        var roles = await user.GetRolesAsync(result);
        var claims = await user.GetClaimsAsync(result);
        return new AppUserDto(result.Id, result.Email!, roles, claims);
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var result = await user.FindByIdAsync(userId.ToString());

        return result?.UserName;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var userInfo = await user.FindByIdAsync(userId.ToString());
        if (userInfo is null)
        {
            return false;
        }
        return await user.IsInRoleAsync(userInfo, role);
    }
}