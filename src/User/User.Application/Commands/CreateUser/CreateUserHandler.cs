using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;
using User.Core.Repositories;

namespace User.Application.Commands.CreateUser;

public class CreateUserHandler
{
    private readonly IUserReadOnlyRepository _readRepository;
    private readonly IUserWriteRepository _writeRepository;

    public CreateUserHandler(
        IUserReadOnlyRepository readRepository,
        IUserWriteRepository writeRepository
    )
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<CreateUserResult>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(command.FirstName))
            return Result<CreateUserResult>.Failure(
                new ValidationError(nameof(command.FirstName), "FirstName cannot be empty")
            );

        if (string.IsNullOrWhiteSpace(command.LastName))
            return Result<CreateUserResult>.Failure(
                new ValidationError(nameof(command.LastName), "LastName cannot be empty")
            );

        if (string.IsNullOrWhiteSpace(command.Email))
            return Result<CreateUserResult>.Failure(
                new ValidationError(nameof(command.Email), "Email cannot be empty")
            );

        var existingUserResult = await _readRepository.GetByEmailAsync(
            command.Email,
            cancellationToken
        );
        if (existingUserResult.IsSuccess)
            return Result<CreateUserResult>.Failure(
                new DuplicateError("User", "Email", command.Email)
            );

        var userResult = Core.Domain.UserEntity.Create(
            command.FirstName,
            command.LastName,
            command.Email
        );

        if (userResult.IsFailure)
            return Result<CreateUserResult>.Failure(userResult.Error);

        var user = userResult.Value;

        var addResult = await _writeRepository.AddAsync(user, cancellationToken);
        if (addResult.IsFailure)
            return Result<CreateUserResult>.Failure(addResult.Error);

        var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
        return saveResult.IsFailure
            ? Result<CreateUserResult>.Failure(saveResult.Error)
            : Result<CreateUserResult>.Success(new CreateUserResult(user.Id));
    }
}
