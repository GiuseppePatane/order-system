using Microsoft.EntityFrameworkCore;
using Product.Core.Repositories;
using Product.Infrastructure.EF;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Product.Infrastructure.Repositories;

/// <summary>
/// Read-only EF implementation for product queries
/// </summary>
public class ProductReadOnlyRepository : IProductReadOnlyRepository
{
    private readonly ProductDbContext _context;

    public ProductReadOnlyRepository(ProductDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<Core.Domain.Product>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (product is null)
                return Result<Core.Domain.Product>.Failure(new NotFoundError(nameof(Core.Domain.Product), id.ToString()));

            return Result<Core.Domain.Product>.Success(product);
        }
        catch (Exception ex)
        {
            return Result<Core.Domain.Product>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<Core.Domain.Product>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result<Core.Domain.Product>.Failure(new ValidationError("sku", "SKU is null or empty"));

        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

            if (product is null)
                return Result<Core.Domain.Product>.Failure(new NotFoundError(nameof(Core.Domain.Product), sku));

            return Result<Core.Domain.Product>.Success(product);
        }
        catch (Exception ex)
        {
            return Result<Core.Domain.Product>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<PagedResult<Core.Domain.Product>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        bool? isActive = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower) ||
                    p.Sku.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<Core.Domain.Product>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Result<PagedResult<Core.Domain.Product>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<Core.Domain.Product>>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<Core.Domain.Product>>> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<Core.Domain.Product>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Core.Domain.Product>>.Failure(new PersistenceError(ex.Message));
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        return await _context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }
}

