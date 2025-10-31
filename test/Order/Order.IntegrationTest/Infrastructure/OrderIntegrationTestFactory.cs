using Order.Infrastructure.EF;
using Test.Shared.Infrastructure;
using Xunit.Abstractions;

namespace Order.IntegrationTest.Infrastructure;

public class OrderIntegrationTestFactory : GrpcIntegrationTestFactory<Program, OrderDbContext>
{
    public OrderIntegrationTestFactory(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override string ConnectionStringKey => "orderdb";
    protected override string DatabaseName => "OrderIntegrationTest";
}
