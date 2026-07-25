using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Hybrid;
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

public class RegisterLaborCommandHandler(ILogger<RegisterLaborCommandHandler> logger, IAppDbContext context, IIdentityService identity, HybridCache cache)
: IRequestHandler<RegisterLaborCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterLaborCommand request, CancellationToken cancellationToken)
    {
        var UserId = await identity.CreateUserAsync(request.email, request.password, request.Roles, request.Claims ?? [], cancellationToken);

        if (UserId.IsError)
        {
            logger.LogWarning("Failed to create User for email {Email} , Errors: {@Errors}", request.email, UserId.Errors);

            return UserId.Errors;
        }

        logger.LogInformation("User created successfully With UserId: {UserId} , Email: {Email}", UserId.Value, request.email);

        var employee = Employee.Create(UserId.Value, request.FirstName, request.LastName);

        if (employee.IsError)
        {
            logger.LogWarning("Failed to create Employee for UserId {UserId} , Errors: {@Errors}", UserId.Value, employee.Errors);

            var DeleteResult = await identity.DeleteUserAsync(UserId.Value);

            if (DeleteResult.IsError)
            {
                logger.LogError("Rollback failed : Could not delete User with UserId {UserId} after Employee creation failure. Errors: {@Errors}", UserId.Value, DeleteResult.Errors);
                return DeleteResult.Errors;
            }

            return employee.Errors;
        }

        await context.Employees.AddAsync(employee.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("Labors", cancellationToken);
        await cache.RemoveByTagAsync("Employees", cancellationToken);

        logger.LogInformation("Employee and User created successfully , With Same Id : {UserId}, Email: {Email}", UserId.Value, request.email);

        return employee.Value.Id;
    }
}