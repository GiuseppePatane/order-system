namespace Order.Core.Repositories;

public interface IOrderReadOnlyRepository
{
    Task<Domain.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Domain.Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(List<Domain.Order> items, int totalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        Domain.OrderStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
