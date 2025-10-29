using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Order.Core.Domain;

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Primary identifier
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Unit price at time of order
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Total price for this line item (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    // Private constructor for EF Core
    private OrderItem() { }

    private OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Creates a new order item
    /// </summary>
    public static Result<OrderItem> Create(Guid productId, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            return Result<OrderItem>.Failure(new ValidationError(nameof(productId), "Product ID is required"));

        if (quantity <= 0)
            return Result<OrderItem>.Failure(new ValidationError(nameof(quantity), "Quantity must be greater than zero"));

        if (unitPrice <= 0)
            return Result<OrderItem>.Failure(new ValidationError(nameof(unitPrice), "Unit price must be greater than zero"));

        var orderItem = new OrderItem(productId, quantity, unitPrice);
        return Result<OrderItem>.Success(orderItem);
    }

    /// <summary>
    /// Updates the quantity
    /// </summary>
    public Result<bool> UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            return Result<bool>.Failure(new ValidationError(nameof(newQuantity), "Quantity must be greater than zero"));

        Quantity = newQuantity;
        return Result<bool>.Success(true);
    }
}
