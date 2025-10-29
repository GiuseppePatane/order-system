using Microsoft.EntityFrameworkCore;
using Order.Core.Domain;
using Order.Core.Repositories;
using Order.Infrastructure.EF;

namespace Order.Infrastructure.Repositories;

public class OrderReadOnlyRepository : IOrderReadOnlyRepository
{
    private readonly OrderDbContext _context;

    public OrderReadOnlyRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Core.Domain.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Core.Domain.Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Core.Domain.Order> items, int totalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id, cancellationToken);
    }
}
