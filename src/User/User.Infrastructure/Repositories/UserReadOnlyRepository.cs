using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;
using User.Core.Domain;
using User.Core.Repositories;
using User.Infrastructure.EF;

namespace User.Infrastructure.Repositories;

public class UserReadOnlyRepository : IUserReadOnlyRepository
{
    private readonly UserDbContext _context;

    public UserReadOnlyRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<UserEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (entity == null)
                return Result<UserEntity>.Failure(new NotFoundError(nameof(UserEntity), id.ToString()));

            return Result<UserEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<UserEntity>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<UserEntity>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<UserEntity>.Failure(new ValidationError("email", "Email is null or empty"));

        try
        {
            var entity = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (entity == null)
                return Result<UserEntity>.Failure(new NotFoundError(nameof(UserEntity), email));

            return Result<UserEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<UserEntity>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<PagedResult<UserEntity>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                query = query.Where(u => u.FirstName.ToLower().Contains(s) || u.LastName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(u => u.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<UserEntity>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };

            return Result<PagedResult<UserEntity>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<UserEntity>>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Id == id, cancellationToken);
    }
}
