using Address.Core.Domain;

namespace Address.Core.Repositories;

/// <summary>
/// Write repository for address mutations
/// </summary>
public interface IAddressWriteRepository
{
    /// <summary>
    /// Adds a new address
    /// </summary>
    Task<AddressEntity> AddAsync(AddressEntity address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing address
    /// </summary>
    Task<AddressEntity> UpdateAsync(AddressEntity address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an address
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsets all default addresses for a user (used before setting a new default)
    /// </summary>
    Task UnsetAllDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
