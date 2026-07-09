using MediatR;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;




public sealed record CreateMakeCommand (string Make,List<CreateVehicleModelCommand> Models) :
IRequest<Result<Guid>>;


public class CreateMakeCommandValidator : AbstractValidator<CreateMakeCommand>
{
    public CreateMakeCommandValidator()
    {
        RuleFor(n=>n.Make).NotEmpty().Must(x=> !string.IsNullOrWhiteSpace(x)).WithMessage("You Must Enter Vehicle Make");
        RuleFor(n=>n.Models).NotNull().Must(n=>n.Count()>0).WithMessage("You Must Enter At Least One Model For Make");
        RuleForEach(n=>n.Models).SetValidator(new CreateVehicleModelCommandValidator());
    }
}


public class CreateMakeCommandHandler(IAppDbContext context,HybridCache cache ,ILogger<CreateMakeCommandHandler> logger)
 : IRequestHandler<CreateMakeCommand,Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMakeCommand request, CancellationToken cancellationToken)
    {
        List<VehicleModel> ListOfVehicleModel =[];

        foreach (var model in request.Models)
        {
            var CreatedModel =VehicleModel.Create(Guid.NewGuid(),model.model);
            if (CreatedModel.IsError)
            {
                logger.LogWarning("The Creation Of New VehicleModel With Name {ModelName} Is Fail",model.model);
                return  CreatedModel.Errors;
            }

            ListOfVehicleModel.Add(CreatedModel.Value);
        }

        var Make =  VehicleMake.Create(Guid.NewGuid(),request.Make,ListOfVehicleModel);
        if (Make.IsError)
        {
            logger.LogWarning("The Creation Of New VehicleMake With Name {MakeName} Is Fail",request.Make);
            return Make.TopError;
        }
        await context.VehicleMakes.AddAsync(Make.Value,cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Add New VehicleMake With id {id} And Name {Name}",Make.Value.Id,Make.Value.Make);

        await cache.RemoveByTagAsync("VMakes",cancellationToken);

        logger.LogInformation("The Hybrid Cache Delete The Tag With Name VMakes");

        return Make.Value.Id ;
    }

}