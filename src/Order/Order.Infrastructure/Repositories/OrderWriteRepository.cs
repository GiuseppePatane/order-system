using Order.Core.Repositories;
using Order.Infrastructure.EF;

namespace Order.Infrastructure.Repositories;

public class OrderWriteRepository : IOrderWriteRepository
{
    private readonly OrderDbContext _context;

    public OrderWriteRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Core.Domain.Order> AddAsync(Core.Domain.Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Core.Domain.Order> UpdateAsync(Core.Domain.Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
