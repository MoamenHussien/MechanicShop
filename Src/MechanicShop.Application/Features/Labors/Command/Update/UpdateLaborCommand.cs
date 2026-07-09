using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record UpdateLaborCommand(Guid id,string FirstName,string LastName,bool IsActive): IRequest<Result<Updated>>;

public class UpdateLaborCommandValidator : AbstractValidator<UpdateLaborCommand>
{
    public UpdateLaborCommandValidator()
    {
        RuleFor(n=>n.id).IdRequired("Labor");
         RuleFor(n => n.FirstName).NotEmpty().WithMessage("First Name is required").MinimumLength(2).WithMessage("First Name must be at least 2 characters").MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(n => n.LastName).NotEmpty().WithMessage("Last Name is required").MinimumLength(2).WithMessage("Last Name must be at least 2 characters").MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleFor(n=>n.IsActive).NotNull().WithMessage("The Labors Status Is Required");
    }
}

public class UpdateLaborCommandHandler(ILogger<UpdateLaborCommandHandler> logger, IAppDbContext context)
: IRequestHandler<UpdateLaborCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateLaborCommand request, CancellationToken cancellationToken)
    {
        var labor = await context.Employees.FindAsync(request.id );
        if (labor is null)
        {
            logger.LogWarning("The Labor Is Not Found , To This Labor Id : {id}",request.id);
            return ApplicationErrors.NotFoundTheLabor;
        }

        var UpdateStatus =labor.Update(request.FirstName,request.LastName,request.IsActive);

        if (UpdateStatus.IsError)
        {
            logger.LogWarning("The Labor Failed To Update To This Id : {id} , This Is Errors : {@errors}",request.id,UpdateStatus.Errors);
            return UpdateStatus.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("The Labor Is Updated Successfully , For This ID : {id}",request.id);

        return Result.Updated;
    }
}
