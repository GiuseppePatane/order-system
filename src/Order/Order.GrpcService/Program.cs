using Order.Application.Extensions;
using Order.GrpcService.Services;
using Order.Infrastructure.Extensions;
using Shared.GrpcInfrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddGrpcWithErrorHandling();

// Add Application and Infrastructure services
builder.Services.AddOrderApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrderGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.MapDefaultEndpoints();

app.Run();

public partial class Program { }
