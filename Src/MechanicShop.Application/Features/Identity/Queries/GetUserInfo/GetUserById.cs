using System.Data.Common;
using System.Numerics;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record GetUserByIdCommand(Guid id) : ICachedQuery<Result<AppUserDto>>
{
    public string CacheKey => $"User-{id}";

    public string[] Tags => ["Users"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

public sealed class GetUserByIdCommandValidator : AbstractValidator<GetUserByIdCommand>
{
    public GetUserByIdCommandValidator()
    {
        RuleFor(n=>n.id).IdRequired("User");
    }
}

public sealed class GetUserByIdCommandHandler(ILogger<GetUserByIdCommandHandler> logger, IIdentityService identity)
: IRequestHandler<GetUserByIdCommand, Result<AppUserDto>>
{
    public async Task<Result<AppUserDto>> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
    {
        var UserInfo = await identity.GetUserByIdAsync(request.id );

        if (UserInfo.IsError)
        {
            logger.LogWarning("Cannot get User info For This Id : {id} , And This Is Errors: {@Errors}",request.id, UserInfo.Errors);
            return UserInfo.Errors;
        }

        logger.LogInformation("User info retrieved successfully for Id: {UserId}", request.id);

        return UserInfo.Value;  
    }
}