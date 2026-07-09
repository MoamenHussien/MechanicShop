using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record CreateMakeCommand (string Make,List<CreateVehicleModelCommand> Models) :
IRequest<Result<VehicleMakeDto>>;
