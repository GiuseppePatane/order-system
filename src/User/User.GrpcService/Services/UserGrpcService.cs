using Grpc.Core;
using Shared.GrpcInfrastructure.Base;
using User.Application.Commands.CreateUser;
using User.Application.Commands.UpdateUser;
using User.Application.Commands.DeleteUser;
using User.Core.Repositories;
using Shared.Core.Domain.Errors;
using User.Protos;


namespace User.GrpcService.Services;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly ILogger<UserGrpcService> _logger;
    private readonly IUserReadOnlyRepository _userRepository;
    private readonly CreateUserHandler _createUserHandler;
    private readonly UpdateUserHandler _updateUserHandler;
    private readonly DeleteUserHandler _deleteUserHandler;
    private readonly GrpcServiceBase<UserGrpcService> _baseService;

    public UserGrpcService(
        ILogger<UserGrpcService> logger,
        IUserReadOnlyRepository userRepository,
        CreateUserHandler createUserHandler,
        UpdateUserHandler updateUserHandler,
        DeleteUserHandler deleteUserHandler)
    {
        _logger = logger;
        _userRepository = userRepository;
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _deleteUserHandler = deleteUserHandler;
        _baseService = new InternalGrpcServiceBase(logger);
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new UserResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var userResult = await _userRepository.GetByIdAsync(userId, context.CancellationToken);

            if (userResult.IsFailure)
            {
                var error = userResult.Error;
                if (error is NotFoundError nf)
                {
                    return new UserResponse
                    {
                        Error = _baseService.CreateNotFoundError("User", request.UserId)
                    };
                }

                return new UserResponse { Error = _baseService.CreateInternalError() };
            }

            var user = userResult.Value;
            return new UserResponse
            {
                Data = MapToUserData(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUser for UserId: {UserId}", request.UserId);
            return new UserResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        try
        {
            var pagedResult = await _userRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                context.CancellationToken);

            if (pagedResult.IsFailure)
            {
                return new GetUsersResponse
                {
                    Error = _baseService.CreateInternalError(pagedResult.Error.Message)
                };
            }

            var data = pagedResult.Value;
            var response = new UserListData
            {
                PageNumber = data.PageNumber,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount
            };

            response.Items.AddRange(data.Items.Select(MapToUserData));

            return new GetUsersResponse { Data = response };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUsers");
            return new GetUsersResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateUserCommand(
                request.FirstName,
                request.LastName,
                request.Email);

            var result = await _createUserHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                if (error is ValidationError ve)
                    return new UserResponse { Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason) };

                if (error is NotFoundError nf)
                    return new UserResponse { Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId) };

                if (error is DuplicateError de)
                    return new UserResponse { Error = _baseService.CreateError(de.Code, de.Message) };

                if (error is PersistenceError pe)
                    return new UserResponse { Error = _baseService.CreateInternalError(pe.Message) };

                return new UserResponse { Error = _baseService.CreateInternalError() };
            }

            return new UserResponse
            {
                Data = new UserData { UserId = result.Value.UserId.ToString() }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateUser");
            return new UserResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new UserResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var command = new UpdateUserCommand(
                userId,
                request.HasFirstName ? request.FirstName : null,
                request.HasLastName ? request.LastName : null,
                request.HasEmail ? request.Email : null);

            var result = await _updateUserHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                if (error is ValidationError ve)
                    return new UserResponse { Error = _baseService.CreateInvalidArgumentError(ve.FieldName, ve.Reason) };

                if (error is NotFoundError nf)
                    return new UserResponse { Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId) };

                if (error is DuplicateError de)
                    return new UserResponse { Error = _baseService.CreateError(de.Code, de.Message) };

                if (error is PersistenceError pe)
                    return new UserResponse { Error = _baseService.CreateInternalError(pe.Message) };

                return new UserResponse { Error = _baseService.CreateInternalError() };
            }

            return new UserResponse
            {
                Data = new UserData { UserId = result.Value.UserId.ToString() }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateUser");
            return new UserResponse { Error = _baseService.CreateInternalError() };
        }
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!_baseService.IsValidGuid(request.UserId, out var userId))
            {
                return new DeleteUserResponse
                {
                    Error = _baseService.CreateInvalidArgumentError("UserId", "invalid or missing GUID")
                };
            }

            var command = new DeleteUserCommand(userId);
            var result = await _deleteUserHandler.Handle(command, context.CancellationToken);

            if (result.IsFailure)
            {
                var error = result.Error;

                if (error is NotFoundError nf)
                    return new DeleteUserResponse { Error = _baseService.CreateNotFoundError(nf.EntityType, nf.EntityId) };

                if (error is PersistenceError pe)
                    return new DeleteUserResponse { Error = _baseService.CreateInternalError(pe.Message) };

                return new DeleteUserResponse { Error = _baseService.CreateInternalError() };
            }

            return new DeleteUserResponse
            {
                Data = new DeleteUserData
                {
                    Success = result.Value.Success,
                    UserId = result.Value.UserId.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeleteUser");
            return new DeleteUserResponse { Error = _baseService.CreateInternalError() };
        }
    }

    private static UserData MapToUserData(Core.Domain.UserEntity user)
    {
        var data = new UserData
        {
            UserId = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = true,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
        };

        if (user.UpdatedAt != default)
        {
            data.UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(user.UpdatedAt, DateTimeKind.Utc));
        }

        return data;
    }

    private class InternalGrpcServiceBase : GrpcServiceBase<UserGrpcService>
    {
        public InternalGrpcServiceBase(ILogger<UserGrpcService> logger) : base(logger) { }
    }
}