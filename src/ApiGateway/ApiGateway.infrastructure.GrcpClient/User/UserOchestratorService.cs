using ApiGateway.Core.Address;
using ApiGateway.Core.Common;
using ApiGateway.Core.Order;
using ApiGateway.Core.User;
using ApiGateway.Core.User.Dto;
using ApiGateway.infrastructure.GrcpClient.Extensions;
using Microsoft.Extensions.Logging;

namespace ApiGateway.infrastructure.GrcpClient.User;

public class UserOchestratorService : IUserOrchestratorService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IAddressServiceClient _addressServiceClient;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly ILogger<UserOchestratorService> _logger;

    public UserOchestratorService(IUserServiceClient userServiceClient, IAddressServiceClient addressServiceClient, IOrderServiceClient orderServiceClient, ILogger<UserOchestratorService> logger)
    {
        _userServiceClient = userServiceClient;
        _addressServiceClient = addressServiceClient;
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    public async Task<ServiceResult<UserMutationResultDto>> DeleteUser(string userId,CancellationToken cancellationToken)
    {

            _logger.LogDebug("Validating existence of user {UserId}", userId);
            var userResult = await _userServiceClient.GetUserById(userId, cancellationToken);

            if (!userResult.IsSuccess || userResult.Data == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return ServiceResult<UserMutationResultDto>.Failure(
                    userResult.Error
                    ?? new ErrorInfo
                    {
                        Code = "USER_NOT_FOUND",
                        Message = $"User with ID {userId} not found",
                    }
                );
            }
            
            DeleteAddresses(userId,cancellationToken).Forget(_logger); // Fire and forget address deletion 
            DeleteOrders(userId,cancellationToken).Forget(_logger); // Fire and forget order deletion
            
    
            _logger.LogDebug("Deleting addresses for user {UserId}", userId);
            
         return  ServiceResult<UserMutationResultDto>.Success(
             new UserMutationResultDto(userId)
             );
    }
    
    private async Task<ServiceResult<UserMutationResultDto>> DeleteAddresses(string userId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting addresses for user {UserId}", userId);
        var addressResult =await  _addressServiceClient.GetAddressesByUser(userId,cancellationToken);
        if (!addressResult.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve addresses for user {UserId}: {Error}", userId, addressResult.Error?.Message);
            return ServiceResult<UserMutationResultDto>.Failure(addressResult.Error!);
        }
        
        if(addressResult.Data != null && addressResult.Data.Any())
        {
            _logger.LogDebug("No addresses to deleted found for user {UserId}", userId);
            return ServiceResult<UserMutationResultDto>.Success(
                new UserMutationResultDto(userId)
            );
        }
        
        foreach (var address in addressResult.Data!)
        {
            _logger.LogDebug("Deleting address {AddressId} for user {UserId}", address.AddressId, userId);
            var deleteAddressResult = await _addressServiceClient.DeleteAddress(address.AddressId, cancellationToken);
            if (!deleteAddressResult.IsSuccess)
            {
                _logger.LogWarning("Failed to delete address {AddressId} for user {UserId}: {Error}", address.AddressId, userId, deleteAddressResult.Error?.Message);
                // ci saranno azioni compensative, per adesso continuiamo a cancellare gli altri indirizzi
                continue;
            }
            _logger.LogDebug("Deleted address {AddressId} for user {UserId}", address.AddressId, userId);
        }
        
        return ServiceResult<UserMutationResultDto>.Success(new  UserMutationResultDto(userId));
    }
    
    private async Task<ServiceResult<UserMutationResultDto>> DeleteOrders(string userId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting orders for user {UserId}", userId);
        var ordersResult = await _orderServiceClient.GetOrdersByUserId(userId,cancellationToken);
        if (!ordersResult.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve orders for user {UserId}: {Error}", userId, ordersResult.Error?.Message);
            return ServiceResult<UserMutationResultDto>.Failure(ordersResult.Error!);
        }
        
        if(ordersResult.Data != null && ordersResult.Data.Any())
        {
            _logger.LogDebug("No orders to deleted found for user {UserId}", userId);
            return ServiceResult<UserMutationResultDto>.Success(
                new UserMutationResultDto(userId)
            );
        }
        
        foreach (var order in ordersResult.Data!)
        {
            _logger.LogDebug("Deleting order {OrderId} for user {UserId}", order.OrderId, userId);
            var deleteOrderResult = await _orderServiceClient.CancelOrder(order.OrderId, "User deletion", cancellationToken);
            if (!deleteOrderResult.IsSuccess)
            {
                _logger.LogWarning("Failed to delete order {OrderId} for user {UserId}: {Error}", order.OrderId, userId, deleteOrderResult.Error?.Message);
                //  ci saranno azioni compensative, per adesso continuiamo a cancellare gli altri ordini
                continue;
            }
            _logger.LogDebug("Deleted order {OrderId} for user {UserId}", order.OrderId, userId);
        }
        
        return ServiceResult<UserMutationResultDto>.Success(new  UserMutationResultDto(userId));
    }
    
    
    
}