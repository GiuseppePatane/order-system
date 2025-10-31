using ApiGateway.Core.Common;
using ApiGateway.Core.User.Dto;

namespace ApiGateway.Core.User;

public interface IUserServiceClient
{
    Task<ServiceResult<UserDto>> GetUserById(string userId, CancellationToken cancellationToken=default);
    
    Task<ServiceResult<PagedUsersDto>> GetUsers(GetUsersRequestDto request);

    Task<ServiceResult<UserMutationResultDto>> CreateUser(CreateUserRequestDto request);
    
    Task<ServiceResult<UserMutationResultDto>> UpdateUser(string id, UpdateUserRequestDto request);

    Task<ServiceResult<UserMutationResultDto>> DeleteUser(string userId);
}
