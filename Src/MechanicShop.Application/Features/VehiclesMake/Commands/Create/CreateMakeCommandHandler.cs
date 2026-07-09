using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public class CreateMakeCommandHandler(IAppDbContext context,HybridCache cache ,ILogger<CreateMakeCommandHandler> logger)
 : IRequestHandler<CreateMakeCommand,Result<VehicleMakeDto>>
{
    async Task<Result<VehicleMakeDto>> IRequestHandler<CreateMakeCommand, Result<VehicleMakeDto>>.Handle
                                (CreateMakeCommand request, CancellationToken cancellationToken)
    {
        List<VehicleModel> ListOfVehicleModel =[];

        foreach (var model in request.Models)
        {
            var CreatedModel =VehicleModel.Create(Guid.NewGuid(),model.model);
            if (CreatedModel.IsError)
            {
                logger.LogWarning("The Creation Of New VehicleModel With Name {ModelName} Is Fail",model.model);
                return CreatedModel.Errors;
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

        return Make.Value.ToMakeDto()  ;
    }
}