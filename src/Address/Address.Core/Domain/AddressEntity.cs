using Shared.Core.Domain.Results;
using Shared.Core.Domain.Errors;

namespace Address.Core.Domain;

/// <summary>
/// Represents a delivery address in the system
/// </summary>
public class AddressEntity
{
    /// <summary>
    /// Primary identifier
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// User ID who owns this address
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Street address line 1
    /// </summary>
    public string Street { get; private set; } = string.Empty;

    /// <summary>
    /// Optional address line 2 (apartment, suite, etc.)
    /// </summary>
    public string? Street2 { get; private set; }

    /// <summary>
    /// City name
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// State or Province
    /// </summary>
    public string State { get; private set; } = string.Empty;

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string PostalCode { get; private set; } = string.Empty;

    /// <summary>
    /// Country name
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>
    /// Optional label for the address (e.g., "Home", "Work")
    /// </summary>
    public string? Label { get; private set; }

    /// <summary>
    /// Indicates if this is the default address for the user
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Timestamp of when the address was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp of when the address was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private AddressEntity() { }

    private AddressEntity(
        Guid userId,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        string? street2 = null,
        string? label = null,
        bool isDefault = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Street = street;
        Street2 = street2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        Label = label;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new address
    /// </summary>
    public static Result<AddressEntity> Create(
        Guid userId,
        string street,
        string city,
        string state,
        string postalCode,
        string country,
        string? street2 = null,
        string? label = null,
        bool isDefault = false)
    {
        // Validation
        if (userId == Guid.Empty)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(userId), "User ID is required"));

        if (string.IsNullOrWhiteSpace(street))
            return Result<AddressEntity>.Failure(new ValidationError(nameof(street), "Street is required"));

        if (street.Length > 200)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(street), "Street cannot exceed 200 characters"));

        if (string.IsNullOrWhiteSpace(city))
            return Result<AddressEntity>.Failure(new ValidationError(nameof(city), "City is required"));

        if (city.Length > 100)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(city), "City cannot exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(state))
            return Result<AddressEntity>.Failure(new ValidationError(nameof(state), "State is required"));

        if (state.Length > 100)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(state), "State cannot exceed 100 characters"));

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result<AddressEntity>.Failure(new ValidationError(nameof(postalCode), "Postal code is required"));

        if (postalCode.Length > 20)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(postalCode), "Postal code cannot exceed 20 characters"));

        if (string.IsNullOrWhiteSpace(country))
            return Result<AddressEntity>.Failure(new ValidationError(nameof(country), "Country is required"));

        if (country.Length > 100)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(country), "Country cannot exceed 100 characters"));

        if (street2?.Length > 200)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(street2), "Street 2 cannot exceed 200 characters"));

        if (label?.Length > 50)
            return Result<AddressEntity>.Failure(new ValidationError(nameof(label), "Label cannot exceed 50 characters"));

        var address = new AddressEntity(userId, street, city, state, postalCode, country, street2, label, isDefault);
        return Result<AddressEntity>.Success(address);
    }

    /// <summary>
    /// Updates the address information
    /// </summary>
    public Result<bool> Update(
        string? street = null,
        string? street2 = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? label = null,
        bool? isDefault = null)
    {
        if (!string.IsNullOrWhiteSpace(street))
        {
            if (street.Length > 200)
                return Result<bool>.Failure(new ValidationError(nameof(street), "Street cannot exceed 200 characters"));
            Street = street;
        }

        if (street2 != null)
        {
            if (street2.Length > 200)
                return Result<bool>.Failure(new ValidationError(nameof(street2), "Street 2 cannot exceed 200 characters"));
            Street2 = street2;
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            if (city.Length > 100)
                return Result<bool>.Failure(new ValidationError(nameof(city), "City cannot exceed 100 characters"));
            City = city;
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            if (state.Length > 100)
                return Result<bool>.Failure(new ValidationError(nameof(state), "State cannot exceed 100 characters"));
            State = state;
        }

        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            if (postalCode.Length > 20)
                return Result<bool>.Failure(new ValidationError(nameof(postalCode), "Postal code cannot exceed 20 characters"));
            PostalCode = postalCode;
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            if (country.Length > 100)
                return Result<bool>.Failure(new ValidationError(nameof(country), "Country cannot exceed 100 characters"));
            Country = country;
        }

        if (label != null)
        {
            if (label.Length > 50)
                return Result<bool>.Failure(new ValidationError(nameof(label), "Label cannot exceed 50 characters"));
            Label = label;
        }

        if (isDefault.HasValue)
        {
            IsDefault = isDefault.Value;
        }

        UpdatedAt = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Sets this address as default
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unsets this address as default
    /// </summary>
    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
