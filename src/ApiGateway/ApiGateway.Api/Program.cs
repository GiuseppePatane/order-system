using System.Reflection;
using ApiGateway.infrastructure.GrcpClient.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

// Configure Problem Details (RFC 7807)
builder.Services.AddProblemDetails();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order System API Gateway",
        Version = "v1",
        Description = "API Gateway for Order System "
    });

    // Enable XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add security definition if needed in future
    // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
});

// Configure gRPC client with service discovery and resilience
builder.Services.AddProductGrpcClient("http://product-grpcservice");
builder.Services.AddUserGrpcClient("http://user-grpcservice");
builder.Services.AddAddressGrpcClient("http://address-grpcservice");


var app = builder.Build();

app.MapDefaultEndpoints();

// Enable Problem Details middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Production System API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Production System API";
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
