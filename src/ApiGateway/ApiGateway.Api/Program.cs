using ApiGateway.infrastructure.GrcpClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure Problem Details (RFC 7807)
builder.Services.AddProblemDetails();

// Configure gRPC client with service discovery and resilience
builder.Services.AddProductGrpcClient("http://product-grpcservice");


var app = builder.Build();

app.MapDefaultEndpoints();

// Enable Problem Details middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
