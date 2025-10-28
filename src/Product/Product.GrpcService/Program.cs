using Product.GrpcService.Services;
using Shared.GrpcInfrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container with error handling interceptor
builder.Services.AddGrpcWithErrorHandling();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();