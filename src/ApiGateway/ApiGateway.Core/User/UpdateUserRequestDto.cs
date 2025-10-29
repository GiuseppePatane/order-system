namespace ApiGateway.Core.User;

public record UpdateUserRequestDto
{
    
    /// <summary>
    /// Campi opzionali da aggiornare
    /// </summary>
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    
}
