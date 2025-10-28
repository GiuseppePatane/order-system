using Bogus;
using Product.Core.Domain;
using Product.Infrastructure.EF;

namespace Product.IntegrationTest.Fixtures;

/// <summary>
/// Generates realistic test data using Bogus
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates test categories
    /// </summary>
    public static List<Category> CreateCategories(int count = 5)
    {
        var categories = new List<Category>();

        var categoryNames = new[]
        {
            "Electronics", "Computers", "Mobile Phones", "Gaming", "Audio",
            "Cameras", "Smart Home", "Wearables", "Accessories", "Office"
        };

        for (int i = 0; i < Math.Min(count, categoryNames.Length); i++)
        {
            var result = Category.Create(
                categoryNames[i],
                _faker.Lorem.Sentence()
            );

            if (result.IsSuccess)
            {
                categories.Add(result.Value);
            }
        }

        return categories;
    }

    /// <summary>
    /// Creates test products
    /// </summary>
    public static List<Core.Domain.Product> CreateProducts(Guid categoryId, int count = 10)
    {
        var products = new List<Core.Domain.Product>();

        for (int i = 0; i < count; i++)
        {
            var name = _faker.Commerce.ProductName();
            var result = Core.Domain.Product.Create(
                name,
                _faker.Commerce.ProductDescription(),
                decimal.Parse(_faker.Commerce.Price(10, 5000)),
                _faker.Random.Int(0, 500),
                GenerateSku(name),
                categoryId
            );

            if (result.IsSuccess)
            {
                products.Add(result.Value);
            }
        }

        return products;
    }

    /// <summary>
    /// Seeds database with test data
    /// </summary>
    public static async Task SeedDatabase(ProductDbContext context, int categoryCount = 3, int productsPerCategory = 5)
    {
        var categories = CreateCategories(categoryCount);
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        foreach (var category in categories)
        {
            var products = CreateProducts(category.Id, productsPerCategory);
            await context.Products.AddRangeAsync(products);
        }

        await context.SaveChangesAsync();
    }

    private static string GenerateSku(string productName)
    {
        var prefix = new string(productName
            .Replace(" ", "")
            .Take(3)
            .ToArray())
            .ToUpper();

        var suffix = _faker.Random.Number(100000, 999999);
        return $"{prefix}-{suffix}";
    }
}
