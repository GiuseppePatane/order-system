using Shared.Core.Domain.Results;

namespace Product.Core.Repositories;

/// <summary>
/// Repository interface for Product aggregate root operations
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its ID
    /// </summary>
    Task<Domain.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    Task<Domain.Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of products with optional filters
    /// </summary>
    Task<PagedResult<Domain.Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        bool? isActive = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products for a specific category
    /// </summary>
    Task<IReadOnlyList<Domain.Product>> GetByCategoryIdAsync(
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

    /// <summary>
    /// Adds a new product
    /// </summary>
    Task AddAsync(Domain.Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    void Update(Domain.Product product);

    /// <summary>
    /// Removes a product
    /// </summary>
    void Remove(Domain.Product product);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


