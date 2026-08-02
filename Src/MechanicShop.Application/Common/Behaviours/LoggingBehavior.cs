using System.Data.Common;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

public class LoggingBehavior<TRequest>(ILogger<TRequest> logger, IUser User, IIdentityService identity) : IRequestPreProcessor<TRequest>
where TRequest : notnull
{
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var userName = string.Empty;

        if (User.Id.HasValue && User.Id != Guid.Empty)
        {
            userName = await identity.GetUserNameAsync(User.Id.Value);
        }

        logger.LogInformation("Request : Name : {RequestName} Values : {@RequestValues} User Id : {Userid} User Name : {UserName}", typeof(TRequest).Name, request, User.Id, userName);
    }
}
