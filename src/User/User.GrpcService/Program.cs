using User.Application.Extensions;
using User.GrpcService.Services;
using User.Infrastructure.Extensions;
using Shared.GrpcInfrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddGrpcWithErrorHandling();


builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddUserApplication();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<UserGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

// Make Program accessible to integration tests
public partial class Program { }