using Product.Core.Repositories;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Product.Application.Commands.ReleaseProductStock;

/// <summary>
/// Handler for releasing previously locked product stock (e.g., when order is cancelled)
/// </summary>
public class ReleaseProductStockHandler
{
    private readonly IProductReadOnlyRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;

    public ReleaseProductStockHandler(
        IProductReadOnlyRepository readRepository,
        IProductWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<ReleaseProductStockResult>> Handle(
        ReleaseProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate quantity
        if (command.Quantity <= 0)
        {
            return Result<ReleaseProductStockResult>.Failure(
                new ValidationError(nameof(command.Quantity), "Quantity must be greater than 0"));
        }

        // Get product
        var productResult = await _readRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result<ReleaseProductStockResult>.Failure(productResult.Error);
        }

        var product = productResult.Value;

        // Release stock (increase available quantity)
        var updateStockResult = product.AddStock(command.Quantity);
        if (updateStockResult.IsFailure)
        {
            return Result<ReleaseProductStockResult>.Failure(updateStockResult.Error);
        }

        // Persist changes
        var updateResult = _writeRepository.Update(product);
        if (updateResult.IsFailure)
        {
            return Result<ReleaseProductStockResult>.Failure(updateResult.Error);
        }

        var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return Result<ReleaseProductStockResult>.Failure(saveResult.Error);
        }

        return Result<ReleaseProductStockResult>.Success(
            new ReleaseProductStockResult(
                product.Id,
                product.Stock,
                command.Quantity));
    }
}

