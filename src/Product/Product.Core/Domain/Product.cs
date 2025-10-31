using Product.Core.Domain.Errors;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Product.Core.Domain;

/// <summary>
///  Domain entity that represents a product
/// </summary>
public class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    
    
    public Category? Category { get; set; }

    // Ef Core constructor
    private Product()
    {
    }

    private Product(string name, string description, decimal price, int stock, string sku, Guid categoryId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        Sku = sku;
        CategoryId = categoryId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///  Create a new product with validation
    /// </summary>
    public static Result<Product> Create(string name, string description, decimal price, int stock, string sku, Guid categoryId)
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(name))
            return Result<Product>.Failure(new ValidationError(nameof(Name), "Name cannot be empty"));

        if (string.IsNullOrWhiteSpace(sku))
            return Result<Product>.Failure(new ValidationError(nameof(Sku), "SKU cannot be empty"));

        if (price <= 0)
            return Result<Product>.Failure(new InvalidPriceError(price));

        if (stock < 0)
            return Result<Product>.Failure(new ValidationError(nameof(Stock), "Stock cannot be negative"));

        var product = new Product(name, description, price, stock, sku, categoryId);
        return Result<Product>.Success(product);
    }


    public Result UpdateStock(int quantity)
    {
        if (Stock + quantity < 0)
            return Result.Failure(new NegativeStockError(Name));

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }


    public Result ReduceStock(int quantity)
    {
        if (!IsActive)
            return Result.Failure(new ProductNotActiveError(Id));

        if (Stock < quantity)
            return Result.Failure(new InsufficientStockError(Name, Stock, quantity));

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }


    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(new ValidationError(nameof(quantity), "Quantity must be greater than zero"));

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }


    public Result<bool> CheckAvailability(int quantity)
    {
        if (!IsActive)
            return Result<bool>.Failure(new ProductNotActiveError(Id));

        bool isAvailable = Stock >= quantity;
        return Result<bool>.Success(isAvailable);
    }


    public Result UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            return Result.Failure(new InvalidPriceError(newPrice));

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

  
    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new ValidationError(nameof(Name), "Name cannot be empty"));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }


    public Result UpdateDescription(string newDescription)
    {
        Description = newDescription ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result UpdateSku(string newSku)
    {
        if (string.IsNullOrWhiteSpace(newSku))
            return Result.Failure(new ValidationError(nameof(Sku), "SKU cannot be empty"));

        Sku = newSku;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result ChangeCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            return Result.Failure(new ValidationError(nameof(CategoryId), "CategoryId cannot be empty"));

        CategoryId = newCategoryId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result SetStock(int newStock)
    {
        if (newStock < 0)
            return Result.Failure(new ValidationError(nameof(Stock), "Stock cannot be negative"));

        Stock = newStock;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}