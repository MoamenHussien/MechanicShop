using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<TRequest> logger, IUser user, IIdentityService identity) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var userName = string.Empty;

            if (user.Id.HasValue && user.Id != Guid.Empty)
            {
                userName = await identity.GetUserNameAsync(user.Id.Value);
            }

            logger.LogWarning(
                "Long running request {RequestName} took {ElapsedMilliseconds} ms. Values: {@RequestValues}. UserName: {UserName}. UserId: {UserId}",
                typeof(TRequest).Name,
                elapsedMilliseconds,
                request,
                userName,
                user.Id?.ToString() ?? "No User");
        }

        return response;
    }
}
