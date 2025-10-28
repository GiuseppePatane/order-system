using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;
using Product.Core.Repositories;

namespace Product.Application.Commands.CreateProduct;

public class CreateProductHandler
{
    private readonly IProductReadOnlyRepository _readRepository;
    private readonly IProductWriteRepository _writeRepository;
    private readonly ICategoryReadOnlyRepository _categoryReadOnlyRepository;

    public CreateProductHandler(
        IProductReadOnlyRepository readRepository,
        IProductWriteRepository writeRepository,
        ICategoryReadOnlyRepository categoryReadOnlyRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _categoryReadOnlyRepository = categoryReadOnlyRepository;
    }

    public async Task<Result<CreateProductResult>> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<CreateProductResult>.Failure(new ValidationError(nameof(command.Name), "Name cannot be empty"));

        if (command.Price <= 0)
            return Result<CreateProductResult>.Failure(new ValidationError(nameof(command.Price), "Price must be greater than zero"));

        if (command.Stock < 0)
            return Result<CreateProductResult>.Failure(new ValidationError(nameof(command.Stock), "Stock cannot be negative"));

        if (string.IsNullOrWhiteSpace(command.Sku))
            return Result<CreateProductResult>.Failure(new ValidationError(nameof(command.Sku), "SKU cannot be empty"));

        // Verify category exists by fetching it
        var categoryResult = await _categoryReadOnlyRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (categoryResult.IsFailure)
            return Result<CreateProductResult>.Failure(categoryResult.Error);

        // category fetched to ensure existence (not used further here)
        _ = categoryResult.Value;

        // Check SKU uniqueness
        var skuExists = await _readRepository.SkuExistsAsync(command.Sku, cancellationToken);
        if (skuExists)
            return Result<CreateProductResult>.Failure(new DuplicateError("Product", "Sku", command.Sku));

        // Create domain product
        var productResult = Core.Domain.Product.Create(
            command.Name,
            command.Description,
            command.Price,
            command.Stock,
            command.Sku,
            command.CategoryId);

        if (productResult.IsFailure)
            return Result<CreateProductResult>.Failure(productResult.Error);

        var product = productResult.Value;

        // Persist
        var addResult = await _writeRepository.AddAsync(product, cancellationToken);
        if (addResult.IsFailure)
            return Result<CreateProductResult>.Failure(addResult.Error);

        var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return Result<CreateProductResult>.Failure(saveResult.Error);

        return Result<CreateProductResult>.Success(new CreateProductResult(product.Id));
    }
}
