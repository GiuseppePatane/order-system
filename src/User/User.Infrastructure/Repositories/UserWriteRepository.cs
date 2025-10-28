using Shared.Core.Domain.Results;
using User.Core.Domain;
using User.Core.Repositories;
using User.Infrastructure.EF;

namespace User.Infrastructure.Repositories;

public class UserWriteRepository : IUserWriteRepository
{
    private readonly UserDbContext _context;

    public UserWriteRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result> AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            return Result.Failure(new Shared.Core.Domain.Errors.ValidationError("user", "User is null"));

        try
        {
            await _context.Users.AddAsync(user, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Shared.Core.Domain.Errors.PersistenceError(ex.Message));
        }
    }

    public Result Update(UserEntity user)
    {
        if (user == null)
            return Result.Failure(new Shared.Core.Domain.Errors.ValidationError("user", "User is null"));

        try
        {
            _context.Users.Update(user);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Shared.Core.Domain.Errors.PersistenceError(ex.Message));
        }
    }

    public Result Remove(UserEntity user)
    {
        if (user == null)
            return Result.Failure(new Shared.Core.Domain.Errors.ValidationError("user", "User is null"));

        try
        {
            _context.Users.Remove(user);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Shared.Core.Domain.Errors.PersistenceError(ex.Message));
        }
    }

    public async Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var changes = await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(changes);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(new Shared.Core.Domain.Errors.PersistenceError(ex.Message));
        }
    }
}

