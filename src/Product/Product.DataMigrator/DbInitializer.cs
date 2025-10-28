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

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No pending migrations.");
            }

            // Seed data
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
        // Check if data already exists
        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // Create categories
        var categories = CreateCategories();
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} categories", categories.Count);

        // Create products for each category
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
            ("Computers", "Laptops, desktops, and computer peripherals"),
            ("Mobile Phones", "Smartphones and mobile accessories"),
            ("Gaming", "Gaming consoles, games, and accessories"),
            ("Audio", "Headphones, speakers, and audio equipment"),
            ("Cameras", "Digital cameras and photography equipment"),
            ("Smart Home", "Smart home devices and IoT products"),
            ("Wearables", "Smartwatches and fitness trackers")
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

        // Computers products
        var computersCategory = categories.First(c => c.Name == "Computers");
        products.AddRange(CreateProductsForCategory(computersCategory, new[]
        {
            ("Gaming Laptop", "High-performance gaming laptop with RTX 4070", 1899.99m, 15),
            ("Ultrabook", "Thin and light ultrabook for professionals", 1299.99m, 30),
            ("Desktop PC", "Custom built desktop PC for gaming", 2499.99m, 10),
            ("Monitor 27\"", "4K IPS Monitor with HDR support", 449.99m, 45),
            ("External SSD 1TB", "Fast external SSD with USB 3.2", 89.99m, 120)
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

        // Gaming products
        var gamingCategory = categories.First(c => c.Name == "Gaming");
        products.AddRange(CreateProductsForCategory(gamingCategory, new[]
        {
            ("Gaming Console", "Next-gen gaming console", 499.99m, 20),
            ("Gaming Headset", "7.1 Surround sound gaming headset", 79.99m, 80),
            ("Controller", "Wireless gaming controller", 59.99m, 100),
            ("Racing Wheel", "Force feedback racing wheel", 299.99m, 25),
            ("VR Headset", "Virtual reality headset bundle", 399.99m, 30)
        }));

        // Audio products
        var audioCategory = categories.First(c => c.Name == "Audio");
        products.AddRange(CreateProductsForCategory(audioCategory, new[]
        {
            ("Noise Cancelling Headphones", "Premium noise cancelling headphones", 349.99m, 60),
            ("Earbuds", "True wireless earbuds with ANC", 149.99m, 200),
            ("Soundbar", "Home theater soundbar with subwoofer", 399.99m, 35),
            ("Studio Monitors", "Professional studio monitor speakers", 599.99m, 20),
            ("Microphone", "USB condenser microphone for streaming", 129.99m, 75)
        }));

        // Cameras products
        var camerasCategory = categories.First(c => c.Name == "Cameras");
        products.AddRange(CreateProductsForCategory(camerasCategory, new[]
        {
            ("Mirrorless Camera", "Professional mirrorless camera body", 1799.99m, 15),
            ("DSLR Camera", "Entry-level DSLR camera kit", 699.99m, 25),
            ("Action Camera", "4K action camera with waterproof case", 299.99m, 50),
            ("Tripod", "Professional carbon fiber tripod", 149.99m, 60),
            ("Camera Lens 50mm", "Prime lens f/1.8 for portraits", 199.99m, 40)
        }));

        // Smart Home products
        var smartHomeCategory = categories.First(c => c.Name == "Smart Home");
        products.AddRange(CreateProductsForCategory(smartHomeCategory, new[]
        {
            ("Smart Speaker", "Voice assistant smart speaker", 99.99m, 150),
            ("Smart Light Bulb", "WiFi RGB smart light bulb", 19.99m, 300),
            ("Security Camera", "Indoor WiFi security camera", 49.99m, 100),
            ("Smart Thermostat", "WiFi programmable thermostat", 179.99m, 50),
            ("Smart Plug", "WiFi smart plug with energy monitoring", 24.99m, 200)
        }));

        // Wearables products
        var wearablesCategory = categories.First(c => c.Name == "Wearables");
        products.AddRange(CreateProductsForCategory(wearablesCategory, new[]
        {
            ("Smartwatch", "Premium smartwatch with fitness tracking", 399.99m, 80),
            ("Fitness Tracker", "Activity and sleep tracker band", 79.99m, 150),
            ("Smart Ring", "Health monitoring smart ring", 299.99m, 40),
            ("Smart Glasses", "AR smart glasses for daily use", 499.99m, 20),
            ("Heart Rate Monitor", "Chest strap heart rate monitor", 59.99m, 100)
        }));

        return products;
    }

    private static List<Core.Domain.Product> CreateProductsForCategory(
        Category category,
        (string Name, string Description, decimal Price, int Stock)[] productData)
    {
        var products = new List<Core.Domain.Product>();
        var faker = new Faker();

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
        // Generate SKU from product name: First 3 letters + random 6 digits
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
