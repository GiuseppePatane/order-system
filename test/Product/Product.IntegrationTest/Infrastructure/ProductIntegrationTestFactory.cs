using Product.Infrastructure.EF;
using Test.Shared.Infrastructure;
using Xunit.Abstractions;

namespace Product.IntegrationTest.Infrastructure;

/// <summary>
/// Integration test factory for Product gRPC service
/// </summary>
public class ProductIntegrationTestFactory : GrpcIntegrationTestFactory<Program, ProductDbContext>
{
    public ProductIntegrationTestFactory(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    /// <summary>
    /// Use "ProductDatabase" as connection string key
    /// </summary>
    protected override string ConnectionStringKey => "ProductDatabase";

    /// <summary>
    /// Use "ProductIntegrationTest" as database name
    /// </summary>
    protected override string DatabaseName => "ProductIntegrationTest";
}
