var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("production-system");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("pgdata")
    .WithImage("postgres:16")
    .WithPgAdmin();

var productDb = postgres.AddDatabase("productdb");
var userDb = postgres.AddDatabase("userdb");
var addressDb = postgres.AddDatabase("addressdb");
var orderDb = postgres.AddDatabase("orderdb");

var productMigration = builder.AddProject<Projects.Product_DataMigrator>("product-migrator")
    .WithReference(productDb);

var userMigration = builder.AddProject<Projects.User_DataMigrator>("user-migrator")
    .WithReference(userDb);

var addressMigration = builder.AddProject<Projects.Address_DataMigrator>("address-migrator")
    .WithReference(addressDb);

var orderMigration = builder.AddProject<Projects.Order_DataMigrator>("order-migrator")
    .WithReference(orderDb);


var productService = builder.AddProject<Projects.Product_GrpcService>("product-grpcservice")
    .WithReference(productDb)
    .WaitForCompletion(productMigration);

var userService = builder.AddProject<Projects.User_GrpcService>("user-grpcservice")
    .WithReference(userDb)
    .WaitForCompletion(userMigration);

var addressService = builder.AddProject<Projects.Address_GrpcService>("address-grpcservice")
    .WithReference(addressDb)
    .WaitForCompletion(addressMigration);

var orderService = builder.AddProject<Projects.Order_GrpcService>("order-grpcservice")
    .WithReference(orderDb)
    .WaitForCompletion(orderMigration);

builder.AddProject<Projects.ApiGateway_Api>("apigateway-api")
    .WithReference(productService)
    .WithReference(userService)
    .WithReference(addressService)
    .WithReference(orderService);

builder.Build().Run();
