
using Product.Core.Repositories;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Product.Application.Commands.LockProductStock;

/// <summary>
/// Handler for locking product stock (e.g., during order creation)
/// </summary>
public class LockProductStockHandler
{
    private readonly IProductReadOnlyRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;

    public LockProductStockHandler(
        IProductReadOnlyRepository readRepository,
        IProductWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<LockProductStockResult>> Handle(
        LockProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate quantity
        if (command.Quantity <= 0)
        {
            return Result<LockProductStockResult>.Failure(
                new ValidationError(nameof(command.Quantity), "Quantity must be greater than 0"));
        }
        
    
                // Get product
                var productResult = await _readRepository.GetByIdAsync(command.ProductId, cancellationToken);
                if (productResult.IsFailure)
                {
                    return Result<LockProductStockResult>.Failure(productResult.Error);
                }

                var product = productResult.Value;

                // Check if product is active
                if (!product.IsActive)
                {
                    return Result<LockProductStockResult>.Failure(
                        new ValidationError(nameof(product.IsActive), $"Product {product.Name} is not active"));
                }

                // Check stock availability
                if (product.Stock < command.Quantity)
                {
                    return Result<LockProductStockResult>.Failure(
                        new ValidationError(
                            nameof(product.Stock),
                            $"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {command.Quantity}"));
                }

                // Lock stock (reduce available quantity)
                var updateStockResult = product.ReduceStock(command.Quantity);
                if (updateStockResult.IsFailure)
                {
                    return Result<LockProductStockResult>.Failure(updateStockResult.Error);
                }

                // Persist changes
                var updateResult = _writeRepository.Update(product);
                if (updateResult.IsFailure)
                {
                    return Result<LockProductStockResult>.Failure(updateResult.Error);
                }

                var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    return Result<LockProductStockResult>.Failure(saveResult.Error);
                }

                return Result<LockProductStockResult>.Success(
                    new LockProductStockResult(
                        ProductId:  product.Id,
                        UpdatedStock:  product.Stock,
                        LockedQuantity: command.Quantity,
                        LockedPrice: product.Price
                        ));
            
         
        }
    
}