var builder = WebApplication.CreateBuilder(args);

builder.Host.AddCustomSerilog();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHub<WorkOrderHub>("/hubs/workorders");
app.Run();
