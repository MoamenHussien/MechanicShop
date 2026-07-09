using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validation = null)
: IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validation is null){ return await next(cancellationToken);};

        var result  = await validation.ValidateAsync(request,cancellationToken);

        if (result.IsValid)
        {
            return await next(cancellationToken);
        }

        var errors = result.Errors.ConvertAll(er=> Error.Validation(er.PropertyName,er.ErrorMessage));

        return (dynamic)errors;
    }
}