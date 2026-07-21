using System.Data;
using System.Net.Http.Headers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

public sealed record UpdateMakeCommand(Guid id ,string Make,List<UpdateModelCommand> Models) :IRequest<Result<Updated>>;
public class UpdateMakeCommandValidator : AbstractValidator<UpdateMakeCommand>
{
    public UpdateMakeCommandValidator()
    {
        RuleFor(n=>n.id).IdRequired("Make");
        RuleFor(n=>n.Make).NotEmpty().Must(x=> !string.IsNullOrWhiteSpace(x)).WithMessage("You Must Enter Vehicle Make");
        RuleFor(n=>n.Models).NotNull().Must(n=> n != null && n.Count()>0).WithMessage("You Must Enter At Least One Model");
        RuleForEach(n=> n.Models).SetValidator(new UpdateModelCommandValidator());
    }
}
public class UpdateMakeCommandHandler(IAppDbContext context,ILogger<CreateMakeCommandHandler> logger ,HybridCache Cache) 
          : IRequestHandler<UpdateMakeCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateMakeCommand request, CancellationToken cancellationToken)
    {
        var Make = await context.VehicleMakes.Include(n=>n.VehicleModels)
                                   .FirstOrDefaultAsync(n=>n.Id==request.id,cancellationToken);
        if (Make is not null)
        {
           var  UpdateMakeInfoResult = Make.Update(request.Make);
           if (UpdateMakeInfoResult.IsError)
            {
                logger.LogWarning("The Update Is failed , to vehicleMake With id {id}",request.id);
                return UpdateMakeInfoResult.TopError;
            }

             List<VehicleModel> UpVehicleModel =[];

             foreach(var model in request.Models)
            {
              var id = model.ModelId ?? Guid.NewGuid();                
              var upModel =VehicleModel.Create(id,model.model);

                if (upModel.IsError)
                {
                    logger.LogWarning("The Process to Create The Model With Id {id} and Name {name}"
                                                                        ,model.ModelId,model.model);
                    return upModel.Errors;
                }
                
                UpVehicleModel.Add(upModel.Value);
            }

            var UpdateModelsResult = Make.UpSertModels(UpVehicleModel);
            if (UpdateModelsResult.IsError)
            {
                logger.LogWarning("Upsert Models failed for Make Id = {id}", request.id);

                return UpdateModelsResult.Errors;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            logger.LogError("The vehicle Make Is Not Found With Id = {id}",request.id);

            return ApplicationErrors.MakeNotFound;
        }

        await Cache.RemoveByTagAsync("VMakes",cancellationToken);

           logger.LogInformation("The Hybrid Cache Delete The Tag With Name VMakes");

        return Result.Updated;
    }
}