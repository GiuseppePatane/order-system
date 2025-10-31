using Product.Core.Domain.Errors;
using Shared.Core.Domain.Errors;
using Shouldly;

namespace Product.UnitTest.Domain;

public class ProductTests
{
    private readonly Guid _categoryId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Laptop";
        var description = "High-performance laptop";
        var price = 1299.99m;
        var stock = 50;
        var sku = "LAP-001";

        // Act
        var result = Core.Domain.Product.Create(name, description, price, stock, sku, _categoryId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
        result.Value.Price.ShouldBe(price);
        result.Value.Stock.ShouldBe(stock);
        result.Value.Sku.ShouldBe(sku);
        result.Value.CategoryId.ShouldBe(_categoryId);
        result.Value.IsActive.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.CreatedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnValidationError(string invalidName)
    {
        // Act
        var result = Core.Domain.Product.Create(invalidName, "Description", 99.99m, 10, "SKU-001", _categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSku_ShouldReturnValidationError(string invalidSku)
    {
        // Act
        var result = Core.Domain.Product.Create("Product", "Description", 99.99m, 10, invalidSku, _categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("Sku");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidPrice_ShouldReturnInvalidPriceError(decimal invalidPrice)
    {
        // Act
        var result = Core.Domain.Product.Create("Product", "Description", invalidPrice, 10, "SKU-001", _categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<InvalidPriceError>();
        var error = result.Error as InvalidPriceError;
        error!.Price.ShouldBe(invalidPrice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNegativeStock_ShouldReturnValidationError(int negativeStock)
    {
        // Act
        var result = Core.Domain.Product.Create("Product", "Description", 99.99m, negativeStock, "SKU-001", _categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("Stock");
    }

    #endregion

    #region UpdateStock Tests

    [Fact]
    public void UpdateStock_WithPositiveQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = CreateValidProduct();
        var initialStock = product.Stock;

        // Act
        var result = product.UpdateStock(10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(initialStock + 10);
        product.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantityButSufficientStock_ShouldDecreaseStock()
    {
        // Arrange
        var product = CreateValidProduct(stock: 50);

        // Act
        var result = product.UpdateStock(-10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(40);
    }

    [Fact]
    public void UpdateStock_WithQuantityThatWouldMakeStockNegative_ShouldReturnError()
    {
        // Arrange
        var product = CreateValidProduct(stock: 5);

        // Act
        var result = product.UpdateStock(-10);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NegativeStockError>();
        product.Stock.ShouldBe(5); // Stock should remain unchanged
    }

    #endregion

    #region ReduceStock Tests

    [Fact]
    public void ReduceStock_WithSufficientStock_ShouldReduceStockSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct(stock: 50);

        // Act
        var result = product.ReduceStock(10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(40);
        product.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void ReduceStock_WithInsufficientStock_ShouldReturnInsufficientStockError()
    {
        // Arrange
        var product = CreateValidProduct(stock: 5);

        // Act
        var result = product.ReduceStock(10);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<InsufficientStockError>();
        var error = result.Error as InsufficientStockError;
        error!.Available.ShouldBe(5);
        error.Requested.ShouldBe(10);
        error.ProductName.ShouldBe(product.Name);
        product.Stock.ShouldBe(5); // Stock should remain unchanged
    }

    [Fact]
    public void ReduceStock_WhenProductIsNotActive_ShouldReturnProductNotActiveError()
    {
        // Arrange
        var product = CreateValidProduct(stock: 50);
        product.Deactivate();

        // Act
        var result = product.ReduceStock(10);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ProductNotActiveError>();
        var error = result.Error as ProductNotActiveError;
        error!.ProductId.ShouldBe(product.Id);
        product.Stock.ShouldBe(50); // Stock should remain unchanged
    }

    #endregion

    #region AddStock Tests

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = CreateValidProduct(stock: 10);

        // Act
        var result = product.AddStock(20);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(30);
        product.UpdatedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AddStock_WithInvalidQuantity_ShouldReturnValidationError(int invalidQuantity)
    {
        // Arrange
        var product = CreateValidProduct();
        var initialStock = product.Stock;

        // Act
        var result = product.AddStock(invalidQuantity);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        product.Stock.ShouldBe(initialStock); // Stock should remain unchanged
    }

    #endregion

    #region CheckAvailability Tests

    [Fact]
    public void CheckAvailability_WhenStockIsSufficient_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateValidProduct(stock: 50);

        // Act
        var result = product.CheckAvailability(10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public void CheckAvailability_WhenStockIsInsufficient_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateValidProduct(stock: 5);

        // Act
        var result = product.CheckAvailability(10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public void CheckAvailability_WhenProductIsNotActive_ShouldReturnError()
    {
        // Arrange
        var product = CreateValidProduct(stock: 50);
        product.Deactivate();

        // Act
        var result = product.CheckAvailability(10);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ProductNotActiveError>();
    }

    #endregion

    #region UpdatePrice Tests

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdateSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = product.UpdatePrice(199.99m);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Price.ShouldBe(199.99m);
        product.UpdatedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void UpdatePrice_WithInvalidPrice_ShouldReturnInvalidPriceError(decimal invalidPrice)
    {
        // Arrange
        var product = CreateValidProduct();
        var originalPrice = product.Price;

        // Act
        var result = product.UpdatePrice(invalidPrice);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<InvalidPriceError>();
        product.Price.ShouldBe(originalPrice); // Price should remain unchanged
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = CreateValidProduct();
        product.IsActive.ShouldBeTrue();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.ShouldBeFalse();
        product.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        product.Deactivate();
        product.IsActive.ShouldBeFalse();

        // Act
        product.Activate();

        // Assert
        product.IsActive.ShouldBeTrue();
        product.UpdatedAt.ShouldNotBeNull();
    }

    #endregion

    #region Helper Methods

    private Core.Domain.Product CreateValidProduct(
        string name = "Test Product",
        string description = "Test Description",
        decimal price = 99.99m,
        int stock = 10,
        string sku = "TEST-001")
    {
        var result = Core.Domain.Product.Create(name, description, price, stock, sku, _categoryId);
        return result.Value;
    }

    #endregion
}