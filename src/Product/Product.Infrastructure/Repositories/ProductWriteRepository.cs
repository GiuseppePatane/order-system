using Product.Core.Repositories;
using Product.Infrastructure.EF;
using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Product.Infrastructure.Repositories;

/// <summary>
/// Write-only EF implementation for product mutations
/// </summary>
public class ProductWriteRepository : IProductWriteRepository
{
    private readonly ProductDbContext _context;

    public ProductWriteRepository(ProductDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result> AddAsync(Core.Domain.Product product, CancellationToken cancellationToken = default)
    {
        if (product == null)
            return Result.Failure(new ValidationError("product", "Product is null"));

        try
        {
            await _context.Products.AddAsync(product, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new PersistenceError(ex.Message));
        }
    }

    public Result Update(Core.Domain.Product product)
    {
        if (product == null)
            return Result.Failure(new ValidationError("product", "Product is null"));

        try
        {
            _context.Products.Update(product);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new PersistenceError(ex.Message));
        }
    }

    public Result Remove(Core.Domain.Product product)
    {
        if (product == null)
            return Result.Failure(new ValidationError("product", "Product is null"));

        try
        {
            _context.Products.Remove(product);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Failure(new PersistenceError(ex.Message));
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
            return Result<int>.Failure(new PersistenceError(ex.Message));
        }
    }
}

