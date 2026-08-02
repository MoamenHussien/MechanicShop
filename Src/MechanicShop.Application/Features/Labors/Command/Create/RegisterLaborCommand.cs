using System.Security.Claims;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record RegisterLaborCommand(string email, string password, string FirstName, string LastName, List<string> Roles, List<Claim> Claims) : IRequest<Result<Guid>>;

public class RegisterLaborCommandValidator : AbstractValidator<RegisterLaborCommand>
{
    public RegisterLaborCommandValidator()
    {
        RuleFor(n => n.email).MustBeValidEmail();
        RuleFor(n => n.password).NotEmpty().WithMessage("Password Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Password").Length(8, 30).WithMessage("Password must be between 8 and 30 characters");
        RuleFor(n => n.FirstName).NotEmpty().WithMessage("First Name is required").MinimumLength(2).WithMessage("First Name must be at least 2 characters").MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(n => n.LastName).NotEmpty().WithMessage("Last Name is required").MinimumLength(2).WithMessage("Last Name must be at least 2 characters").MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleForEach(n => n.Roles).IsEnumName(typeof(Role), caseSensitive: false).WithMessage("Roles must be a valid Enum Value");
        RuleFor(n => n.Claims).NotNull().WithMessage("The Claims Must Be Not Null");
    }
}

public class RegisterLaborCommandHandler(ILogger<RegisterLaborCommandHandler> logger, IAppDbContext context, IIdentityService identity, ICacheInvalidator cacheInvalidator)
: IRequestHandler<RegisterLaborCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterLaborCommand request, CancellationToken cancellationToken)
    {
        var userId = await identity.CreateUserAsync(request.email, request.password, request.Roles, request.Claims ?? [], cancellationToken);

        if (userId.IsError)
        {
            logger.LogWarning("Failed to create User for email {Email} , Errors: {@Errors}", request.email, userId.Errors);

            return userId.Errors;
        }

        logger.LogInformation("User created successfully With UserId: {UserId} , Email: {Email}", userId.Value, request.email);

        var employee = Employee.Create(userId.Value, request.FirstName, request.LastName);

        if (employee.IsError)
        {
            logger.LogWarning("Failed to create Employee for UserId {UserId} , Errors: {@Errors}", userId.Value, employee.Errors);

            var deleteResult = await identity.DeleteUserAsync(userId.Value);

            if (deleteResult.IsError)
            {
                logger.LogError("Rollback failed : Could not delete User with UserId {UserId} after Employee creation failure. Errors: {@Errors}", userId.Value, deleteResult.Errors);
                return deleteResult.Errors;
            }

            return employee.Errors;
        }

        await context.Employees.AddAsync(employee.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cacheInvalidator.EvictByTagAsync(CacheTags.Users, cancellationToken);

        logger.LogInformation("Employee and User created successfully , With Same Id : {UserId}, Email: {Email}", userId.Value, request.email);

        return employee.Value.Id;
    }
}
