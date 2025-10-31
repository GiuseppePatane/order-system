using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Core.Domain;
using Product.Infrastructure.EF;

namespace Product.DataMigrator;

/// <summary>
/// Initializes the database with migrations and seed data
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(ProductDbContext context, ILogger logger)
    {
        try
        {
            // Apply migrations
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            var migrations = pendingMigrations.ToList();
            if (migrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", migrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No pending migrations.");
            }

            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task SeedDataAsync(ProductDbContext context, ILogger logger)
    {
    
        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");
        
        var categories = CreateCategories();
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} categories", categories.Count);
        
        var products = CreateProducts(categories);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} products", products.Count);
        logger.LogInformation("Database seeding completed successfully.");
    }

    private static List<Category> CreateCategories()
    {
        var categories = new List<Category>();

        var categoryData = new[]
        {
            ("Electronics", "Electronic devices and accessories"),
            ("Mobile Phones", "Smartphones and mobile accessories"),
        };

        foreach (var (name, description) in categoryData)
        {
            var result = Category.Create(name, description);
            if (result.IsSuccess)
            {
                categories.Add(result.Value);
            }
        }

        return categories;
    }

    private static List<Core.Domain.Product> CreateProducts(List<Category> categories)
    {
        var products = new List<Core.Domain.Product>();
        var faker = new Faker();

        // Electronics products
        var electronicsCategory = categories.First(c => c.Name == "Electronics");
        products.AddRange(CreateProductsForCategory(electronicsCategory, new[]
        {
            ("LED TV 55\"", "4K Ultra HD Smart LED Television", 799.99m, 25),
            ("Bluetooth Speaker", "Portable Wireless Bluetooth Speaker", 49.99m, 150),
            ("USB-C Hub", "7-in-1 USB-C Hub with HDMI and Ethernet", 39.99m, 200),
            ("Wireless Mouse", "Ergonomic Wireless Mouse with USB Receiver", 24.99m, 300),
            ("Mechanical Keyboard", "RGB Mechanical Gaming Keyboard", 129.99m, 75)
        }));
        

        // Mobile Phones products
        var mobilesCategory = categories.First(c => c.Name == "Mobile Phones");
        products.AddRange(CreateProductsForCategory(mobilesCategory, new[]
        {
            ("Smartphone Pro", "Flagship smartphone with 5G", 999.99m, 50),
            ("Budget Smartphone", "Affordable smartphone with great features", 299.99m, 100),
            ("Phone Case", "Protective phone case with kickstand", 19.99m, 500),
            ("Screen Protector", "Tempered glass screen protector", 9.99m, 600),
            ("Wireless Charger", "Fast wireless charging pad", 34.99m, 200)
        }));
        
        

        return products;
    }

    private static List<Core.Domain.Product> CreateProductsForCategory(
        Category category,
        (string Name, string Description, decimal Price, int Stock)[] productData)
    {
        var products = new List<Core.Domain.Product>();

        foreach (var (name, description, price, stock) in productData)
        {
            var sku = GenerateSku(name);
            var result = Core.Domain.Product.Create(
                name,
                description,
                price,
                stock,
                sku,
                category.Id);

            if (result.IsSuccess)
            {
                products.Add(result.Value);
            }
        }

        return products;
    }

    private static string GenerateSku(string productName)
    {
        // Generate SKU from product name: First 3 letters + random 6 digits ( e.g., ELE-123456 )
        var prefix = new string(productName
            .Replace(" ", "")
            .Take(3)
            .ToArray())
            .ToUpper();

        var random = new Random();
        var suffix = random.Next(100000, 999999);

        return $"{prefix}-{suffix}";
    }
}
