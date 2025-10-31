using System.Reflection;
using ApiGateway.Core.Product.Dto.Validator;
using ApiGateway.infrastructure.GrcpClient.Extensions;
using FluentValidation;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestDtoValidator>();

builder.Services.AddControllers();


builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order System API Gateway",
        Version = "v1",
        Description = "API Gateway for Order System "
    });
    
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure gRPC client with service discovery and resilience
builder.Services.AddProductGrpcClient("http://product-grpcservice");
builder.Services.AddUserGrpcClient("http://user-grpcservice");
builder.Services.AddAddressGrpcClient("http://address-grpcservice");
builder.Services.AddOrderGrpcClient("http://order-grpcservice");


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
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.Run();
