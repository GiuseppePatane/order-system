namespace ApiGateway.Core.User.Dto;

public record UpdateUserRequestDto
{
    
    /// <summary>
    /// Campi opzionali da aggiornare
    /// </summary>
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    
}
