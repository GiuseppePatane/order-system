using Shared.Core.Domain.Results;
using User.Core.Domain;

namespace User.Core.Repositories;

public interface IUserReadOnlyRepository
{
    Task<Result<UserEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserEntity>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<UserEntity>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

