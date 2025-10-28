using Microsoft.EntityFrameworkCore;
using Product.Core.Domain;
using Product.Infrastructure.EF;
using Product.IntegrationTest.Fixtures;
using Product.IntegrationTest.Infrastructure;
using Products;
using Shouldly;
using Xunit.Abstractions;

namespace Product.IntegrationTest;

/// <summary>
/// Integration tests for ProductGrpcService
/// </summary>
[Collection("ProductGrpc")]
public class ProductGrpcServiceTests : IAsyncLifetime
{
    private readonly ProductIntegrationTestFactory _factory;
    private ProductService.ProductServiceClient _grpcClient = null!;

    public ProductGrpcServiceTests(ITestOutputHelper testOutputHelper)
    {
        _factory = new ProductIntegrationTestFactory(testOutputHelper);
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _grpcClient = new ProductService.ProductServiceClient(_factory.Channel);
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        
        Guid productId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var categories = TestDataGenerator.CreateCategories(1);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = TestDataGenerator.CreateProducts(categories[0].Id, 1);
            context.Products.AddRange(products);
            context.SaveChanges();

            productId = products[0].Id;
        });

        // Act
        var request = new GetProductRequest { ProductId = productId.ToString() };
        var response = await _grpcClient.GetProductAsync(request);

        // Assert
        response.ResultCase.ShouldBe(ProductResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.ProductId.ShouldBe(productId.ToString());
        response.Data.Name.ShouldNotBeEmpty();
        response.Data.Price.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnError()
    {
        // Arrange
       
        var nonExistentId = Guid.NewGuid();

        // Act
        var request = new GetProductRequest { ProductId = nonExistentId.ToString() };
        var response = await _grpcClient.GetProductAsync(request);

        // Assert
        response.ResultCase.ShouldBe(ProductResponse.ResultOneofCase.Error);
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldContain("NOT_FOUND");
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedList()
    {
        // Arrange

        await _factory.SeedDatabaseAsync(context =>
        {
            var categories = TestDataGenerator.CreateCategories(2);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            foreach (var category in categories)
            {
                var products = TestDataGenerator.CreateProducts(category.Id, 10);
                context.Products.AddRange(products);
            }
            context.SaveChanges();
        });

        // Act
        var request = new GetProductsRequest
        {
            PageNumber = 1,
            PageSize = 5
        };
        var response = await _grpcClient.GetProductsAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetProductsResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
        response.Data.Items.Count.ShouldBe(5);
        response.Data.PageNumber.ShouldBe(1);
        response.Data.PageSize.ShouldBe(5);
        response.Data.TotalCount.ShouldBe(20);
        response.Data.TotalPages.ShouldBe(4);
        
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
       

        Guid targetCategoryId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var categories = TestDataGenerator.CreateCategories(3);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            targetCategoryId = categories[0].Id;

            // Add 5 products to first category, 10 to others
            var products1 = TestDataGenerator.CreateProducts(categories[0].Id, 5);
            var products2 = TestDataGenerator.CreateProducts(categories[1].Id, 10);
            var products3 = TestDataGenerator.CreateProducts(categories[2].Id, 10);

            context.Products.AddRange(products1);
            context.Products.AddRange(products2);
            context.Products.AddRange(products3);
            context.SaveChanges();
        });

        // Act
        var request = new GetProductsRequest
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = targetCategoryId.ToString()
        };
        var response = await _grpcClient.GetProductsAsync(request);

        // Assert
        response.ResultCase.ShouldBe(GetProductsResponse.ResultOneofCase.Data);
        response.Data.Items.Count.ShouldBe(5);
        response.Data.TotalCount.ShouldBe(5);
        response.Data.Items.All(p => p.CategoryId == targetCategoryId.ToString()).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
       

        Guid categoryId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var categories = TestDataGenerator.CreateCategories(1);
            context.Categories.AddRange(categories);
            context.SaveChanges();
            categoryId = categories[0].Id;
        });

        // Act
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99,
            Stock = 100,
            Sku = "TEST-001",
            CategoryId = categoryId.ToString()
        };
        var response = await _grpcClient.CreateProductAsync(request);

        // Assert
        response.ResultCase.ShouldBe(ProductResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();
       response.Data.ProductId.ShouldNotBeNullOrEmpty();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var savedProduct = await context.Products
            .FirstOrDefaultAsync(p => p.Sku == "TEST-001");

        savedProduct.ShouldNotBeNull();
        savedProduct.Name.ShouldBe("Test Product");
        savedProduct.Price.ShouldBe(99.99m);
        savedProduct.Stock.ShouldBe(100);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
       

        Guid productId = Guid.Empty;
        List<Category> categories= [];
        await _factory.SeedDatabaseAsync(context =>
        {
            categories = TestDataGenerator.CreateCategories(2);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = TestDataGenerator.CreateProducts(categories[0].Id, 1);
            context.Products.AddRange(products);
            context.SaveChanges();

            productId = products[0].Id;
        });

        // Act
        var request = new UpdateProductRequest
        {
            ProductId = productId.ToString(),
            Name = "Updated Product Name",
            Description = "Updated Description",
            CategoryId = categories[1].Id.ToString(),
            Price = 199.99,
            Stock = 150,
            Sku = "UPDATED-001"
            
        };
        var response = await _grpcClient.UpdateProductAsync(request);

        // Assert
        response.ResultCase.ShouldBe(ProductResponse.ResultOneofCase.Data);
        response.Data.ShouldNotBeNull();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var updatedProduct = await context.Products.FindAsync(productId);

        updatedProduct.ShouldNotBeNull();
        updatedProduct.Name.ShouldBe("Updated Product Name");
        updatedProduct.Price.ShouldBe(199.99m);
        updatedProduct.CategoryId.ShouldBe(categories[1].Id);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldDeleteProduct()
    {
        // Arrange
       

        Guid productId = Guid.Empty;
        await _factory.SeedDatabaseAsync(context =>
        {
            var categories = TestDataGenerator.CreateCategories(1);
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = TestDataGenerator.CreateProducts(categories[0].Id, 1);
            context.Products.AddRange(products);
            context.SaveChanges();

            productId = products[0].Id;
        });

        // Act
        var request = new DeleteProductRequest { ProductId = productId.ToString() };
        var response = await _grpcClient.DeleteProductAsync(request);

        // Assert
        response.ResultCase.ShouldBe(DeleteProductResponse.ResultOneofCase.Data);
        response.Data.Success.ShouldBeTrue();

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var deletedProduct = await context.Products.FindAsync(productId);

        deletedProduct.ShouldBeNull();
    }
}
