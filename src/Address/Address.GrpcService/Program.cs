using Address.Application.Extensions;
using Address.GrpcService.Services;
using Address.Infrastructure.Extensions;
using Shared.GrpcInfrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpcWithErrorHandling();

builder.Services.AddAddressInfrastructure(builder.Configuration);
builder.Services.AddAddressApplication();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGrpcService<AddressGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
