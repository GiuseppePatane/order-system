namespace ApiGateway.Core.Product;

public record UpdateProductRequestDto
{
    /// <summary>
    /// Id del prodotto (obbligatorio)
    /// </summary>
    public required string ProductId { get; init; }

    /// <summary>
    /// Campi opzionali da aggiornare
    /// </summary>
    public string? Name { get; init; }
    public string? Description { get; init; }
    public double? Price { get; init; }
    public string? Sku { get; init; }
    public string? CategoryId { get; init; }

    /// <summary>
    /// Validates the request and returns a list of errors (empty if valid)
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProductId) || !Guid.TryParse(ProductId, out _))
            errors.Add("ProductId is required and must be a valid GUID");

        if (Price.HasValue && Price <= 0)
            errors.Add("Price must be greater than zero when specified");

        if (!string.IsNullOrWhiteSpace(CategoryId) && !Guid.TryParse(CategoryId, out _))
            errors.Add("CategoryId must be a valid GUID when specified");

        return errors;
    }
}
