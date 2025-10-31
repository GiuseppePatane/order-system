using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Order.Core.Domain;

/// <summary>
/// Represents an order in the system
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    /// <summary>
    /// Primary identifier
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// User ID who placed the order
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Order items
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Shipping address ID
    /// </summary>
    public Guid ShippingAddressId { get; private set; }

    /// <summary>
    /// Optional billing address ID 
    /// </summary>
    public Guid? BillingAddressId { get; private set; }

    /// <summary>
    /// Total amount for the order
    /// </summary>
    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);

    /// <summary>
    /// Order status
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Timestamp of when the order was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp of when the order was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Reason for cancellation (if cancelled)
    /// </summary>
    public string? CancellationReason { get; private set; }

    // Private constructor for EF Core
    private Order() { }

    private Order(Guid userId, Guid shippingAddressId, Guid? billingAddressId, List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ShippingAddressId = shippingAddressId;
        BillingAddressId = billingAddressId;
        _items = items;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    public static Result<Order> Create(
        Guid userId,
        Guid shippingAddressId,
        List<OrderItem> items,
        Guid? billingAddressId = null)
    {
        // Validation
        if (userId == Guid.Empty)
            return Result<Order>.Failure(new ValidationError(nameof(userId), "User ID is required"));

        if (shippingAddressId == Guid.Empty)
            return Result<Order>.Failure(new ValidationError(nameof(shippingAddressId), "Shipping address ID is required"));

        if (items == null || items.Count == 0)
            return Result<Order>.Failure(new ValidationError(nameof(items), "Order must contain at least one item"));

        var order = new Order(userId, shippingAddressId, billingAddressId, items);
        return Result<Order>.Success(order);
    }

    /// <summary>
    /// Updates the order status
    /// </summary>
    public Result<bool> UpdateStatus(OrderStatus newStatus)
    {
        // Validate status transitions
        var validTransition = (Status, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Confirmed, OrderStatus.Processing) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            _ when Status == newStatus => false, 
            _ => false
        };

        if (!validTransition)
            return Result<bool>.Failure(new ValidationError(
                nameof(newStatus),
                $"Cannot transition from {Status} to {newStatus}"));

        if (Status != newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Cancels the order
    /// </summary>
    public Result<bool> Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Delivered)
            return Result<bool>.Failure(new ValidationError(
                nameof(Status),
                "Cannot cancel a delivered order"));

        if (Status == OrderStatus.Cancelled)
            return Result<bool>.Failure(new ValidationError(
                nameof(Status),
                "Order is already cancelled"));

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Adds an item to the order. If the order is in the pending status.
    /// </summary>
    public Result<bool> AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            return Result<bool>.Failure(new ValidationError(
                nameof(Status),
                "Can only add items to pending orders"));

        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Removes an item from the order (only if pending)
    /// </summary>
    public Result<bool> RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            return Result<bool>.Failure(new ValidationError(
                nameof(Status),
                "Can only remove items from pending orders"));

        if (_items.Count == 1)  //todo: validazione paranoica, forse conviene cancellare l'ordine direttamente.
            return Result<bool>.Failure(new ValidationError(
                nameof(_items),
                "Order must have at least one item"));
        
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return Result<bool>.Failure(new NotFoundError("OrderItem", itemId.ToString()));

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }
}
