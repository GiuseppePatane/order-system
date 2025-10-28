using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;
using Product.Core.Repositories;

namespace Product.Application.Commands.UpdateProduct;

public class UpdateProductHandler
{
    private readonly IProductReadOnlyRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;
    private readonly ICategoryReadOnlyRepository _categoryReadOnlyRepository;

    public UpdateProductHandler(
        IProductReadOnlyRepository readRepository,
        IProductWriteRepository writeRepository,
        ICategoryReadOnlyRepository categoryReadOnlyRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _categoryReadOnlyRepository = categoryReadOnlyRepository;
    }

    public async Task<Result<UpdateProductResult>> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        // fetch product
        var prodResult = await _readRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (prodResult.IsFailure)
            return Result<UpdateProductResult>.Failure(prodResult.Error);

        var product = prodResult.Value;

        // Name
        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            var r = product.UpdateName(command.Name);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        // Description
        if (command.Description != null)
        {
            var r = product.UpdateDescription(command.Description);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        // Price
        if (command.Price.HasValue)
        {
            var r = product.UpdatePrice(command.Price.Value);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        // Stock
        if (command.Stock.HasValue)
        {
            var r = product.SetStock(command.Stock.Value);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        // SKU
        if (!string.IsNullOrWhiteSpace(command.Sku) && command.Sku != product.Sku)
        {
            var exists = await _readRepository.SkuExistsAsync(command.Sku, cancellationToken);
            if (exists)
                return Result<UpdateProductResult>.Failure(new DuplicateError("Product", "Sku", command.Sku));

            var r = product.UpdateSku(command.Sku);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        // Category
        if (command.CategoryId.HasValue && command.CategoryId.Value != product.CategoryId)
        {
            var catResult = await _categoryReadOnlyRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken);
            if (catResult.IsFailure) return Result<UpdateProductResult>.Failure(catResult.Error);

            var r = product.ChangeCategory(command.CategoryId.Value);
            if (r.IsFailure) return Result<UpdateProductResult>.Failure(r.Error);
        }

        var updateRes = _writeRepository.Update(product);
        if (updateRes.IsFailure) return Result<UpdateProductResult>.Failure(updateRes.Error);

        var saveRes = await _writeRepository.SaveChangesAsync(cancellationToken);
        if (saveRes.IsFailure) return Result<UpdateProductResult>.Failure(saveRes.Error);

        return Result<UpdateProductResult>.Success(new UpdateProductResult(product.Id));
    }
}
