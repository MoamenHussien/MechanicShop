using MediatR;
using Microsoft.Extensions.Logging;

public class UnhandledExceptionBehavior<TRequest, TResponse>(Logger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string   RequestName = typeof(TRequest).Name;
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"Request : UnHandle Exception For Request {RequestName} With Value {@Request}",RequestName,request);
            throw;
        }
    }
}