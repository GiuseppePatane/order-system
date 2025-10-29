using Address.Core.Domain;

namespace Address.Core.Repositories;

/// <summary>
/// Read-only repository for address queries
/// </summary>
public interface IAddressReadOnlyRepository
{
    /// <summary>
    /// Gets an address by ID
    /// </summary>
    Task<AddressEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all addresses for a specific user
    /// </summary>
    Task<IEnumerable<AddressEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default address for a user
    /// </summary>
    Task<AddressEntity?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of addresses for a user
    /// </summary>
    Task<(IEnumerable<AddressEntity> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an address exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
