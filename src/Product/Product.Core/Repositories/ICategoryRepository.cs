using Product.Core.Domain;
using Shared.Core.Domain.Results;

namespace Product.Core.Repositories;

/// <summary>
/// Repository interface for Category aggregate root operations
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets a category by its ID
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its name
    /// </summary>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Adds a new category
    /// </summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    void Update(Category category);

    /// <summary>
    /// Removes a category
    /// </summary>
    void Remove(Category category);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
