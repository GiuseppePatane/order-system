using Shared.Core.Domain.Results;
using Product.Core.Repositories;

namespace Product.Application.Commands.DeleteProduct;

public class DeleteProductHandler
{
    private readonly IProductReadOnlyRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;

    public DeleteProductHandler(IProductReadOnlyRepository readRepository, IProductWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<DeleteProductResult>> Handle(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var prodResult = await _readRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (prodResult.IsFailure)
            return Result<DeleteProductResult>.Failure(prodResult.Error);

        var product = prodResult.Value;

        var rem = _writeRepository.Remove(product);
        if (rem.IsFailure) return Result<DeleteProductResult>.Failure(rem.Error);

        var save = await _writeRepository.SaveChangesAsync(cancellationToken);
        if (save.IsFailure) return Result<DeleteProductResult>.Failure(save.Error);

        return Result<DeleteProductResult>.Success(new DeleteProductResult(true, product.Id));
    }
}
