using FluentValidation;
using MechanicShop.Application.Common.Constants;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed record CreateMakeCommand(string Make, List<CreateVehicleModelCommand> Models) :
IRequest<Result<Guid>>;

public class CreateMakeCommandValidator : AbstractValidator<CreateMakeCommand>
{
    public CreateMakeCommandValidator()
    {
        RuleFor(n => n.Make).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("You Must Enter Vehicle Make");
        RuleFor(n => n.Models).NotNull().Must(n => n != null && n.Count() > 0).WithMessage("You Must Enter At Least One Model For Make");
        RuleForEach(n => n.Models).SetValidator(new CreateVehicleModelCommandValidator());
    }
}

public class CreateMakeCommandHandler(IAppDbContext context, ICacheInvalidator cacheInvalidator, ILogger<CreateMakeCommandHandler> logger)
 : IRequestHandler<CreateMakeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = request.Make.CapitalizeFirstLetter();
        var isMakeExits = await context.VehicleMakes.AnyAsync(n => n.Make == make, cancellationToken);

        if (isMakeExits)
        {
            logger.LogWarning("This Make Vehicle Is Already Exits : {Make}", request.Make);
            return VehicleMakeErrors.MakeIsAlreadyExists;
        }

        List<VehicleModel> listOfVehicleModel = [];

        foreach (var model in request.Models)
        {
            var createdModel = VehicleModel.Create(Guid.NewGuid(), model.model);
            if (createdModel.IsError)
            {
                logger.LogWarning("The Creation Of New VehicleModel With Name {ModelName} Is Fail", model.model);
                return createdModel.Errors;
            }

            listOfVehicleModel.Add(createdModel.Value);
        }

        var make1 = VehicleMake.Create(Guid.NewGuid(), request.Make, listOfVehicleModel);
        if (make1.IsError)
        {
            logger.LogWarning("The Creation Of New VehicleMake With Name {MakeName} Is Fail", request.Make);
            return make1.TopError;
        }

        await context.VehicleMakes.AddAsync(make1.Value, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Add New VehicleMake With id {id} And Name {Name}", make1.Value.Id, make1.Value.Make);

        await cacheInvalidator.EvictByTagAsync(CacheTags.VehicleMakes, cancellationToken);

        logger.LogInformation("The Hybrid Cache Delete The Tag With Name VMakes");

        return make1.Value.Id;
    }
}
