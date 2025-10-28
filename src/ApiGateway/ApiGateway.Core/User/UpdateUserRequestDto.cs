namespace ApiGateway.Core.User;

public record UpdateUserRequestDto
{
    /// <summary>
    /// Id dell'utente (obbligatorio)
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Campi opzionali da aggiornare
    /// </summary>
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }

    /// <summary>
    /// Validates the request and returns a list of errors (empty if valid)
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(UserId) || !Guid.TryParse(UserId, out _))
            errors.Add("UserId is required and must be a valid GUID");

        return errors;
    }
}
