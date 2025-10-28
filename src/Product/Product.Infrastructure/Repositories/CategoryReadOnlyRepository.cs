using Microsoft.EntityFrameworkCore;
using Product.Core.Domain;
using Product.Core.Repositories;
using Product.Infrastructure.EF;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Product.Infrastructure.Repositories;

public class CategoryReadOnlyRepository : ICategoryReadOnlyRepository
{
    private readonly ProductDbContext _context;

    public CategoryReadOnlyRepository(ProductDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<Category>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.Categories.
                FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (category == null)
                return Result<Category>.Failure(new NotFoundError(nameof(Category), id.ToString()));

            return Result<Category>.Success(category);
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<Category>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Category>.Failure(new ValidationError(nameof(name), "Name cannot be empty"));

        try
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

            if (category == null)
                return Result<Category>.Failure(new NotFoundError(nameof(Category), name));

            return Result<Category>.Success(category);
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Category>> GetPagedAsync(int pageNumber, int pageSize, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Categories.AsQueryable();
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);
    }
}