using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;
using User.Core.Repositories;

namespace User.Application.Commands.UpdateUser;

public class UpdateUserHandler
{
    private readonly IUserReadOnlyRepository _readRepository;
    private readonly IUserWriteRepository _writeRepository;

    public UpdateUserHandler(
        IUserReadOnlyRepository readRepository,
        IUserWriteRepository writeRepository
    )
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<UpdateUserResult>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var userResult = await _readRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure)
            return Result<UpdateUserResult>.Failure(userResult.Error);

        var user = userResult.Value;

        if (!string.IsNullOrWhiteSpace(command.Email) && command.Email != user.Email)
        {
            var existingUserResult = await _readRepository.GetByEmailAsync(
                command.Email,
                cancellationToken
            );
            if (existingUserResult.IsSuccess)
                return Result<UpdateUserResult>.Failure(
                    new DuplicateError("User", "Email", command.Email)
                );
        }

        var updateResult = user.Update(
            command.FirstName ?? user.FirstName,
            command.LastName ?? user.LastName,
            command.Email ?? user.Email
        );

        if (updateResult.IsFailure)
            return Result<UpdateUserResult>.Failure(updateResult.Error);

        var updateRepoResult = _writeRepository.Update(user);
        if (updateRepoResult.IsFailure)
            return Result<UpdateUserResult>.Failure(updateRepoResult.Error);

        var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
        return saveResult.IsFailure
            ? Result<UpdateUserResult>.Failure(saveResult.Error)
            : Result<UpdateUserResult>.Success(new UpdateUserResult(user.Id));
    }
}
