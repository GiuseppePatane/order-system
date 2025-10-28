using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Product.Core.Domain;

/// <summary>
/// Entità di dominio che rappresenta una categoria di prodotti
/// </summary>
public class Category
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }


    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // ef core constructor
    private Category()
    {
    }

    private Category(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create a new category with validation
    /// </summary>
    public static Result<Category> Create(string name, string description)
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(name))
            return Result<Category>.Failure(new ValidationError(nameof(Name), "Name cannot be empty"));

        var category = new Category(name, description);
        return Result<Category>.Success(category);
    }

    /// <summary>
    ///  Update the category name
    /// </summary>
    /// <param name="newName"></param>
    /// <returns></returns>
    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new ValidationError(nameof(Name), "Name cannot be empty"));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    /// <summary>
    /// Update the category description
    /// </summary>
    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disable the category
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enable the category
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}