namespace Shared.Core.Domain.Errors;

/// <summary>
/// Classe base astratta per tutti gli errori di dominio.
/// Ogni bounded context può estendere questa classe per creare i propri errori specifici.
/// </summary>
public abstract record DomainError(string Code, string Message);

/// <summary>
/// Generic validation error
/// </summary>
public record ValidationError(string FieldName, string Reason)
    : DomainError("VALIDATION_ERROR", $"Validation failed for '{FieldName}': {Reason}");

/// <summary>
/// Error when an entity is not found
/// </summary>
public record NotFoundError(string EntityType, string EntityId)
    : DomainError("NOT_FOUND", $"{EntityType} with ID '{EntityId}' was not found");

/// <summary>
/// Error when there is a duplicate entity
/// </summary>
public record DuplicateError(string EntityType, string Field, string Value)
    : DomainError("DUPLICATE", $"{EntityType} with {Field} '{Value}' already exists");

/// <summary>
/// Error for persistence / infrastructure failures
/// </summary>
public record PersistenceError(string Message)
    : DomainError("PERSISTENCE_ERROR", Message);
