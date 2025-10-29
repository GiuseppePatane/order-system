using System.Text.RegularExpressions;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace User.Core.Domain;

public class UserEntity
{
    private UserEntity(string firstName, string lastName, string email, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    private UserEntity() { }

    public static Result<UserEntity> Create(
        string firstName,
        string lastName,
        string email,
        Guid? id = null
    )
    {
        var validationResult = Validate(firstName, lastName, email);
        if (validationResult.IsFailure)
            return validationResult;
        return id.HasValue
            ? Result<UserEntity>.Success(new UserEntity(firstName, lastName, email, id.Value))
            : Result<UserEntity>.Success(new UserEntity(firstName, lastName, email));
    }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Result<UserEntity> Update(string firstName, string lastName, string email)
    {
        var validationResult = Validate(firstName, lastName, email);
        if (validationResult.IsFailure)
            return validationResult;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
        return Result<UserEntity>.Success(this);
    }

    private static Result<UserEntity> Validate(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<UserEntity>.Failure(
                new ValidationError(nameof(FirstName), "FirstName cannot be empty")
            );
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<UserEntity>.Failure(
                new ValidationError(nameof(LastName), "lastName be empty")
            );
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<UserEntity>.Failure(
                new ValidationError(nameof(Email), "Email cannot be empty")
            );
        }
        if (!IsValidEmail(email))
        {
            return Result<UserEntity>.Failure(
                new ValidationError(nameof(Email), "Invalid email format")
            );
        }

        return Result<UserEntity>.Success(new UserEntity());
    }

    private static bool IsValidEmail(string email)
    {
        return UserExtensions.EmailRegex().IsMatch(email);
    }
}

internal static partial class UserExtensions
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    internal static partial Regex EmailRegex();
}
