var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("production-system");

var productService = builder.AddProject<Projects.Product_GrpcService>("product-grpcservice");

builder.AddProject<Projects.ApiGateway_Api>("apigateway-api")
    .WithReference(productService);

builder.Build().Run();
