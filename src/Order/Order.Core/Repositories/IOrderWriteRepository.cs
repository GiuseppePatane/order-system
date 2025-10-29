namespace Order.Core.Repositories;

public interface IOrderWriteRepository
{
    Task<Domain.Order> AddAsync(Domain.Order order, CancellationToken cancellationToken = default);
    Task<Domain.Order> UpdateAsync(Domain.Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
