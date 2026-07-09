using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class LaborAssignedRequirement : IAuthorizationRequirement;
public class LaborAssignedRequirementHandler(AppDbContext contextDb, HttpContextAccessor httpContextAccessor) 
                                                : AuthorizationHandler<LaborAssignedRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,LaborAssignedRequirement requirement)
    {
        if (context.User.IsInRole(Role.Manager.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToGuid();

        if (userId is null || userId.IsError)
        {
            context.Fail();
            return;
        }

        var workOrderId = httpContextAccessor.HttpContext?.Request.RouteValues["workOrderId"]?.ToString().ToGuid();

        if (workOrderId is null || workOrderId.IsError)
        {
            context.Fail();
            return;
        }

        var isAssigned = await contextDb.WorkOrders.AnyAsync(n => n.Id == workOrderId.Value && n.LaborId == userId.Value);

        if (isAssigned)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}

