using Order.Core.Domain;
using Shared.Core.Domain.Errors;
using Shouldly;
using Xunit;

namespace Order.UnitTest.Domain;

public class OrderItemTests
{
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var quantity = 3;
        var unitPrice = 15.50m;

        // Act
        var result = OrderItem.Create(_productId, quantity, unitPrice);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.ProductId.ShouldBe(_productId);
        result.Value.Quantity.ShouldBe(quantity);
        result.Value.UnitPrice.ShouldBe(unitPrice);
        result.Value.TotalPrice.ShouldBe(46.50m);
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldFail()
    {
        // Act
        var result = OrderItem.Create(Guid.Empty, 1, 10.00m);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("productId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithInvalidQuantity_ShouldFail(int invalidQuantity)
    {
        // Act
        var result = OrderItem.Create(_productId, invalidQuantity, 10.00m);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-10.50)]
    public void Create_WithInvalidUnitPrice_ShouldFail(decimal invalidPrice)
    {
        // Act
        var result = OrderItem.Create(_productId, 1, invalidPrice);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("unitPrice");
    }

    [Fact]
    public void TotalPrice_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var quantity = 5;
        var unitPrice = 12.75m;

        // Act
        var orderItem = OrderItem.Create(_productId, quantity, unitPrice).Value!;

        // Assert
        orderItem.TotalPrice.ShouldBe(63.75m);
    }

    [Fact]
    public void TotalPrice_WithDecimalQuantityAndPrice_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var quantity = 7;
        var unitPrice = 9.99m;

        // Act
        var orderItem = OrderItem.Create(_productId, quantity, unitPrice).Value!;

        // Assert
        orderItem.TotalPrice.ShouldBe(69.93m);
    }
}
