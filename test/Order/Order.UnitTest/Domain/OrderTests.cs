using Order.Core.Domain;
using Shared.Core.Domain.Errors;
using Shouldly;
using Xunit;

namespace Order.UnitTest.Domain;

public class OrderTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _shippingAddressId = Guid.NewGuid();
    private readonly Guid _billingAddressId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 2, 10.50m).Value!
        };

        // Act
        var result = Core.Domain.Order.Create(
            _userId,
            _shippingAddressId,
            items,
            _billingAddressId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.UserId.ShouldBe(_userId);
        result.Value.ShippingAddressId.ShouldBe(_shippingAddressId);
        result.Value.BillingAddressId.ShouldBe(_billingAddressId);
        result.Value.Status.ShouldBe(OrderStatus.Pending);
        result.Value.Items.Count.ShouldBe(1);
        result.Value.TotalAmount.ShouldBe(21.00m);
        result.Value.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        result.Value.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_WithoutBillingAddress_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 15.00m).Value!
        };

        // Act
        var result = Core.Domain.Order.Create(
            _userId,
            _shippingAddressId,
            items);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.BillingAddressId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };

        // Act
        var result = Core.Domain.Order.Create(
            Guid.Empty,
            _shippingAddressId,
            items,
            _billingAddressId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("userId");
    }

    [Fact]
    public void Create_WithEmptyItems_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>();

        // Act
        var result = Core.Domain.Order.Create(
            _userId,
            _shippingAddressId,
            items,
            _billingAddressId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("items");
    }

    [Fact]
    public void Create_WithEmptyShippingAddressId_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };

        // Act
        var result = Core.Domain.Order.Create(
            _userId,
            Guid.Empty,
            items,
            _billingAddressId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var error = result.Error as ValidationError;
        error!.FieldName.ShouldBe("shippingAddressId");
    }

    [Fact]
    public void UpdateStatus_ToPending_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Pending);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }
    


    [Fact]
    public void UpdateStatus_ToConfirmed_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Confirmed);
        order.UpdatedAt.ShouldNotBeNull();
        order.UpdatedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void UpdateStatus_FromCancelledToProcessing_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.Cancel("Test cancellation");

        // Act
        var result = order.UpdateStatus(OrderStatus.Processing);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Cancel_WithReason_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        var cancellationReason = "Customer requested cancellation";

        // Act
        var result = order.Cancel(cancellationReason);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancellationReason.ShouldBe(cancellationReason);
        order.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Cancel_WithoutReason_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.CancellationReason.ShouldBeNull();
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.Cancel("First cancellation");

        // Act
        var result = order.Cancel("Second cancellation");

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void AddItem_WithValidItem_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        var newItem = OrderItem.Create(Guid.NewGuid(), 2, 15.00m).Value!;
        var initialTotal = order.TotalAmount;

        // Act
        var result = order.AddItem(newItem);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.Count.ShouldBe(2);
        order.TotalAmount.ShouldBe(initialTotal + 30.00m);
    }

    [Fact]
    public void RemoveItem_WithExistingItem_ShouldSucceed()
    {
        // Arrange
        var item1 = OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!;
        var item2 = OrderItem.Create(Guid.NewGuid(), 2, 15.00m).Value!;
        var items = new List<OrderItem> { item1, item2 };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.RemoveItem(item1.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.Count.ShouldBe(1);
        order.Items.First().Id.ShouldBe(item2.Id);
        order.TotalAmount.ShouldBe(30.00m);
    }

    [Fact]
    public void RemoveItem_WithNonExistingItem_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!
        };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.RemoveItem(Guid.NewGuid());

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RemoveItem_LeavingEmptyOrder_ShouldFail()
    {
        // Arrange
        var item = OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value!;
        var items = new List<OrderItem> { item };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.RemoveItem(item.Id);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void TotalAmount_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 2, 10.50m).Value!,
            OrderItem.Create(Guid.NewGuid(), 1, 25.75m).Value!,
            OrderItem.Create(Guid.NewGuid(), 3, 5.00m).Value!
        };

        // Act
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Assert
        order.TotalAmount.ShouldBe(61.75m); // (2*10.50) + (1*25.75) + (3*5.00)
    }


    // ============================================
    // COMPLETE UPDATE STATUS TRANSITION TESTS
    // ============================================

    [Fact]
    public void UpdateStatus_PendingToConfirmed_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Confirmed);
        order.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateStatus_ConfirmedToProcessing_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);

        // Act
        var result = order.UpdateStatus(OrderStatus.Processing);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Processing);
    }

    [Fact]
    public void UpdateStatus_ProcessingToShipped_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);

        // Act
        var result = order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Shipped);
    }

    [Fact]
    public void UpdateStatus_ShippedToDelivered_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        // Act
        var result = order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateStatus_PendingToCancelled_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateStatus_ConfirmedToCancelled_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);

        // Act
        var result = order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateStatus_ProcessingToCancelled_ShouldSucceed()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);

        // Act
        var result = order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    // Invalid transitions

    [Fact]
    public void UpdateStatus_SameStatus_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Pending);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_PendingToProcessing_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Processing);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_PendingToShipped_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_PendingToDelivered_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;

        // Act
        var result = order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_ShippedToCancelled_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);

        // Act
        var result = order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_DeliveredToAnyStatus_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act & Assert
        order.UpdateStatus(OrderStatus.Pending).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Confirmed).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Processing).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Shipped).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Cancelled).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_CancelledToAnyStatus_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Cancelled);

        // Act & Assert
        order.UpdateStatus(OrderStatus.Confirmed).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Processing).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Shipped).IsFailure.ShouldBeTrue();
        order.UpdateStatus(OrderStatus.Delivered).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_ConfirmedToPending_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);

        // Act
        var result = order.UpdateStatus(OrderStatus.Pending);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_ProcessingToPending_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Processing);

        // Act
        var result = order.UpdateStatus(OrderStatus.Pending);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStatus_ConfirmedToShipped_ShouldFail()
    {
        // Arrange
        var items = new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.00m).Value! };
        var order = Core.Domain.Order.Create(_userId, _shippingAddressId, items).Value!;
        order.UpdateStatus(OrderStatus.Confirmed);

        // Act
        var result = order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }
}
