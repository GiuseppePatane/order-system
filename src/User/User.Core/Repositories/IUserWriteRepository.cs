using Shared.Core.Domain.Results;
using User.Core.Domain;

namespace User.Core.Repositories;

/// <summary>
/// Write-only repository interface for User aggregate root operations
/// </summary>
public interface IUserWriteRepository
{
    /// <summary>
    /// Adds a new user
    /// </summary>
    Task<Result> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Result Update(UserEntity user);

    /// <summary>
    /// Removes a user
    /// </summary>
    Result Remove(UserEntity user);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
