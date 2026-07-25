using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class IdentityService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> user, AppDbContext context, ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct)
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

    public async Task<Result<Guid>> CreateUserAsync(string email, string password, IList<string> roles, IList<Claim> claims, CancellationToken ct)
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

        if (claims.Count > 0)
        {
            var addClaimsResult = await user.AddClaimsAsync(userinfo, claims);

            if (!addClaimsResult.Succeeded)
            {
                await user.DeleteAsync(userinfo);
                return Error.Conflict("Add_Claims_Failed", string.Join(", ", addClaimsResult.Errors.Select(x => x.Description)));
            }
        }

        var addRolesResult = await user.AddToRolesAsync(userinfo, roles);

        if (!addRolesResult.Succeeded)
        {
            await user.DeleteAsync(userinfo);
            return Error.Conflict("Add_Roles_Failed", string.Join(", ", addRolesResult.Errors.Select(x => x.Description)));
        }
        return userinfo.Id;
    }

    public void DeleteRefreshTokenCookie()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
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
            return Error.Conflict("Delete_User_Failed", string.Join(", ", deleteResult.Errors.Select(x => x.Description)));
        }

        return Result.Success;
    }

    public async Task<Result<List<string>>> GetAllRolesAsync(CancellationToken ct = default)
    {
        var roles = await context.Roles
            .Where(r => r.Name != null && r.Name != Role.Manager.ToString())
            .Select(r => r.Name!)
            .ToListAsync(ct);
            
        if (roles.Count == 0)
        {
            return Error.NotFound("Roles_Not_Found", "Roles not found");
        }

        return roles;
    }

    public async Task<Result<HashSet<Guid>>> GetIdsOfUsersByRoleTypeAsync(Role role)
    {
        var users = await user.GetUsersInRoleAsync(role.ToString());
        var ids = users.Select(n => n.Id).ToHashSet();
        return ids;
    }

    public Result<string> GetRefreshTokenFromCookies()
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Error.Failure("Infrastructure.HttpContext.Unavailable", "HttpContext is not available.");
        }

        var refreshToken = httpContext.Request.Cookies["RefreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Error.Unauthorized("Auth.RefreshToken.Missing", "Refresh token cookie is missing.");
        }

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

    public async Task<Result<Success>> ResetUserPasswordAsync(Guid userId)
    {
        var userInfo = await user.FindByIdAsync(userId.ToString());
        if (userInfo is null)
        {
            return Error.NotFound("User_Not_Found", "User not found.");
        }

        if (string.IsNullOrWhiteSpace(userInfo.Email) || userInfo.Email.Length < 6)
        {
            return Error.Validation("Invalid_Email_Length", "User email must be at least 6 characters long to be used as a password.");
        }

        var token = await user.GeneratePasswordResetTokenAsync(userInfo);
        var newPassword = userInfo.Email;

        var resetResult = await user.ResetPasswordAsync(userInfo, token, newPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return Error.Failure("Password_Reset_Failed", errors);
        }

        return Result.Success;
    }

    public async Task<Result<Success>> UpdateUserPasswordAsync(Guid userid, string newPassword, string currentPassword, CancellationToken ct)
    {
        var userInfo = await user.FindByIdAsync(userid.ToString());
        if (userInfo is null)
        {
            return Error.NotFound("User_Not_Found", "User not found.");
        }

        var changeResult = await user.ChangePasswordAsync(userInfo, currentPassword, newPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join(", ", changeResult.Errors.Select(x => x.Description));
            return Error.Conflict("Password_Change_Failed", errors);
        }

        return Result.Success;
    }

    public async Task<Result<bool>> UpdateUserPermissionsAsync(Guid userId, IList<string> roles, IList<Claim> claims, CancellationToken ct)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
        {
            return Error.NotFound("User_Not_Found", "User not found.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            var newRoleIds = await context.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync(ct);

            var currentUserRoles = await context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(ct);
            var currentUserClaims = await context.UserClaims.Where(uc => uc.UserId == userId).ToListAsync(ct);

            // (Roles)

            var currentUserRoleIds = currentUserRoles.Select(ur => ur.RoleId).ToList();

            var currentUserrolesToRemove = currentUserRoles.Where(ur => !newRoleIds.Contains(ur.RoleId)).ToList();
            var rolesToAdd = newRoleIds.Except(currentUserRoleIds)
                .Select(roleId => new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId })
                .ToList();

            if (currentUserrolesToRemove.Count > 0) context.UserRoles.RemoveRange(currentUserrolesToRemove);
            if (rolesToAdd.Count > 0) context.UserRoles.AddRange(rolesToAdd);

            //(Claims) 

            var claimsToRemove = currentUserClaims
                .Where(c => !claims.Any(nc => nc.Type == c.ClaimType && nc.Value == c.ClaimValue))
                .ToList();

            var claimsToAdd = claims
                .Where(nc => !currentUserClaims.Any(c => c.ClaimType == nc.Type && c.ClaimValue == nc.Value))
                .Select(nc => new IdentityUserClaim<Guid> { UserId = userId, ClaimType = nc.Type, ClaimValue = nc.Value })
                .ToList();

            if (claimsToRemove.Count > 0) context.UserClaims.RemoveRange(claimsToRemove);
            if (claimsToAdd.Count > 0) context.UserClaims.AddRange(claimsToAdd);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update roles and claims for user {UserId}", userId);

            return Error.Failure("Identity.UpdateUserInfo", "An unexpected error occurred while updating the user.");
        }
    }
}