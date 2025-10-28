using Shared.Core.Domain.Results;

namespace Product.Core.Repositories;

/// <summary>
/// Write-only repository interface for Product aggregate root operations
/// </summary>
public interface IProductWriteRepository
{
    /// <summary>
    /// Adds a new product
    /// </summary>
    Task<Result> AddAsync(Domain.Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    Result Update(Domain.Product product);

    /// <summary>
    /// Removes a product
    /// </summary>
    Result Remove(Domain.Product product);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
