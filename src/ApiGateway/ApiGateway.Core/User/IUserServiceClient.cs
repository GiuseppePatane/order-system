using ApiGateway.Core.Common;

namespace ApiGateway.Core.User;

public interface IUserServiceClient
{
    Task<ServiceResult<UserDto>> GetUserById(string userId);

    Task<ServiceResult<UserDto>> CreateUser(CreateUserRequestDto request);

    Task<ServiceResult<PagedUsersDto>> GetUsers(GetUsersRequestDto request);

    Task<ServiceResult<UserDto>> UpdateUser(UpdateUserRequestDto request);

    Task<ServiceResult<DeleteUserResultDto>> DeleteUser(string userId);
}
