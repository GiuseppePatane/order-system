var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("production-system");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("pgdata")
    .WithImage("postgres:16")
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");

var productMigration = builder.AddProject<Projects.Product_DataMigrator>("catalog-migrator")
    .WithReference(productDb);


var productService = builder.AddProject<Projects.Product_GrpcService>("product-grpcservice")
    .WithReference(productDb)
    .WaitForCompletion(productMigration);

builder.AddProject<Projects.ApiGateway_Api>("apigateway-api")
    .WithReference(productService);

builder.Build().Run();
