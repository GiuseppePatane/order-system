using ApiGateway.Core.Common;
using ApiGateway.Core.User.Dto;

namespace ApiGateway.Core.User;

public interface IUserOrchestratorService
{
    Task<ServiceResult<UserMutationResultDto>> DeleteUser(string userId,CancellationToken cancellationToken);
}