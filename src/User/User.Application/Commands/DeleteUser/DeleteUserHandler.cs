using Shared.Core.Domain.Results;
using User.Core.Repositories;

namespace User.Application.Commands.DeleteUser;

public class DeleteUserHandler
{
    private readonly IUserReadOnlyRepository _readRepository;
    private readonly IUserWriteRepository _writeRepository;

    public DeleteUserHandler(
        IUserReadOnlyRepository readRepository,
        IUserWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<DeleteUserResult>> Handle(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
   
        var userResult = await _readRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure)
            return Result<DeleteUserResult>.Failure(userResult.Error);

        var user = userResult.Value;

  
        var removeResult = _writeRepository.Remove(user);
        if (removeResult.IsFailure)
            return Result<DeleteUserResult>.Failure(removeResult.Error);

        var saveResult = await _writeRepository.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return Result<DeleteUserResult>.Failure(saveResult.Error);

        return Result<DeleteUserResult>.Success(new DeleteUserResult(true, user.Id));
    }
}

