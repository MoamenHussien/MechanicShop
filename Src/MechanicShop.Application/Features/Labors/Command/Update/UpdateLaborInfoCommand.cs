using System.Security.Claims;
using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record UpdateLaborInfoCommand(Guid id, string FirstName, string LastName, bool IsActive) : IRequest<Result<Updated>>;

public class UpdateLaborInfoCommandValidator : AbstractValidator<UpdateLaborInfoCommand>
{
    public UpdateLaborInfoCommandValidator()
    {
        RuleFor(n => n.id).IdRequired("Labor");
        RuleFor(n => n.FirstName).NotEmpty().WithMessage("First Name is required").MinimumLength(2).WithMessage("First Name must be at least 2 characters").MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(n => n.LastName).NotEmpty().WithMessage("Last Name is required").MinimumLength(2).WithMessage("Last Name must be at least 2 characters").MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleFor(n => n.IsActive).NotNull().WithMessage("The Labors Status Is Required");
    }
}

public class UpdateLaborInfoCommandHandler(ILogger<UpdateLaborInfoCommandHandler> logger, IAppDbContext context, ICacheInvalidator cacheInvalidator)
: IRequestHandler<UpdateLaborInfoCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateLaborInfoCommand request, CancellationToken cancellationToken)
    {
        var labor = await context.Employees.FindAsync(new object[] { request.id }, cancellationToken);
        if (labor is null)
        {
            logger.LogWarning("The Labor Is Not Found , To This Labor Id : {id}", request.id);
            return ApplicationErrors.NotFoundTheLabor;
        }

        var updateStatus = labor.Update(request.FirstName, request.LastName, request.IsActive);

        if (updateStatus.IsError)
        {
            logger.LogWarning("The Labor Failed To Update To This Id : {id} , This Is Errors : {@errors}", request.id, updateStatus.Errors);
            return updateStatus.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.EvictByTagsAsync(cancellationToken, CacheTags.Labors);
        logger.LogInformation("The Labor Is Updated Successfully , For This ID : {id}", request.id);

        return Result.Updated;
    }
}
