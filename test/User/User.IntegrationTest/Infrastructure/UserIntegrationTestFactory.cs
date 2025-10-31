using User.Infrastructure.EF;
using Test.Shared.Infrastructure;
using Xunit.Abstractions;

namespace User.IntegrationTest.Infrastructure;

/// <summary>
/// Integration test factory for User gRPC service
/// </summary>
public class UserIntegrationTestFactory : GrpcIntegrationTestFactory<Program, UserDbContext>
{
    public UserIntegrationTestFactory(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    /// <summary>
    /// Use "UserDatabase" as connection string key
    /// </summary>
    protected override string ConnectionStringKey => "UserDatabase";

    /// <summary>
    /// Use "UserIntegrationTest" as database name
    /// </summary>
    protected override string DatabaseName => "UserIntegrationTest";
}

