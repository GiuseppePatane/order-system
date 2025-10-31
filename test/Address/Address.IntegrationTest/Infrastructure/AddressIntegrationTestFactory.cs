using Address.Infrastructure.EF;
using Test.Shared.Infrastructure;
using Xunit.Abstractions;

namespace Address.IntegrationTest.Infrastructure;

/// <summary>
/// Integration test factory for Address gRPC service
/// </summary>
public class AddressIntegrationTestFactory : GrpcIntegrationTestFactory<Program, AddressDbContext>
{
    public AddressIntegrationTestFactory(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    /// <summary>
    /// Use "AddressDatabase" as connection string key
    /// </summary>
    protected override string ConnectionStringKey => "AddressDatabase";

    /// <summary>
    /// Use "AddressIntegrationTest" as database name
    /// </summary>
    protected override string DatabaseName => "AddressIntegrationTest";
}
