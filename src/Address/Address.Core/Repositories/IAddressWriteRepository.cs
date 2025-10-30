using Address.Core.Domain;
using Shared.Core.Domain.Results;

namespace Address.Core.Repositories;

/// <summary>
/// Write repository for address mutations
/// </summary>
public interface IAddressWriteRepository
{
    /// <summary>
    /// Adds a new address
    /// </summary>
    Task<Result> AddAsync(AddressEntity address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing address
    /// </summary>
    Task<Result> UpdateAsync(AddressEntity address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an address
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsets the default addresses for a user 
    /// </summary>
    Task<Result> UnsetAllDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a specific address as default for a user (unsets all other defaults)
    /// </summary>
    Task<Result> SetDefaultAddressAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);
}
