using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<TRequest> logger,IUser user,IIdentityService identity) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull{
    Stopwatch stopwatch  = new Stopwatch();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        

        stopwatch.Start();
        var response =  await next(cancellationToken);
        stopwatch.Stop();

        var TakenTime = stopwatch.ElapsedMilliseconds;


        if (TakenTime > 500)
        {
            var username =string.Empty;

            if (user.Id.HasValue && user.Id != Guid.Empty)
            {
                 username = await identity.GetUserNameAsync(user.Id.Value); 
            }

            logger.LogWarning("Long Running , This Request {RequestName} Take {Millisecond} Millisecond With Values {@RequestValues} With UserName {username} with Userid {Userid}",typeof(TRequest).Name,TakenTime,request,username,user.Id.ToString()??"No User");
        }

        return response;
    }
}