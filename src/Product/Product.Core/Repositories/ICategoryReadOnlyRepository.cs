using Product.Core.Domain;
using Shared.Core.Domain.Results;

namespace Product.Core.Repositories;

/// <summary>
/// Repository interface for Category aggregate root operations
/// </summary>
public interface ICategoryReadOnlyRepository
{
    /// <summary>
    /// Gets a category by its ID
    /// </summary>
    Task<Result<Category>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its name
    /// </summary>
    Task<Result<Category>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active categories
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all categories (active and inactive)
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of categories
    /// </summary>
    Task<PagedResult<Category>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category name is already in use
    /// </summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}
