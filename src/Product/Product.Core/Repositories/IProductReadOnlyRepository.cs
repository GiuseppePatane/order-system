using Shared.Core.Domain.Results;

namespace Product.Core.Repositories;

/// <summary>
/// Read-only repository interface for Product aggregate root operations
/// </summary>
public interface IProductReadOnlyRepository
{
    /// <summary>
    /// Gets a product by its ID
    /// </summary>
    Task<Result<Domain.Product>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    Task<Result<Domain.Product>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of products with optional filters
    /// </summary>
    Task<Result<PagedResult<Domain.Product>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        bool? isActive = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products for a specific category
    /// </summary>
    Task<Result<IReadOnlyList<Domain.Product>>> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the given SKU exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a SKU is already in use
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);
}
