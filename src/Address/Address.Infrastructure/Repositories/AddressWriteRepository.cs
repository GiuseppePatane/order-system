using Microsoft.EntityFrameworkCore;
using Address.Core.Domain;
using Address.Core.Repositories;
using Address.Infrastructure.EF;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Errors;
using Shared.Core.Domain.Results;

namespace Address.Infrastructure.Repositories;

public class AddressWriteRepository : IAddressWriteRepository
{
    private readonly ILogger<AddressWriteRepository> _logger;
    private readonly AddressDbContext _context;

    public AddressWriteRepository(AddressDbContext context, ILogger<AddressWriteRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<Result> AddAsync(AddressEntity address, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding new address for user {UserId}", address.UserId);
            await _context.Addresses.AddAsync(address, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Address {AddressId} added to user {UserId}", address.Id, address.UserId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding address for user {UserId}", address.UserId);
            return Result.Failure(new PersistenceError(e.Message));
        }
       
    }

    public async Task<Result> UpdateAsync(AddressEntity address, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating address {AddressId} for user {UserId}", address.Id, address.UserId);
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Address {AddressId} updated for user {UserId}", address.Id, address.UserId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating address {AddressId} for user {UserId}", address.Id, address.UserId);
            return Result.Failure(new PersistenceError(e.Message));
        }
       
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {

        try
        {
            _logger.LogInformation("Deleting address {AddressId}", id);
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
            if (address == null)
            {
                _logger.LogWarning("Address {AddressId} not found for deletion", id);
                return Result.Ok();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting address {AddressId}", id);
            return Result.Failure(new PersistenceError(e.Message));
        }
    }

    public async Task<Result> UnsetAllDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultAddresses = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken: cancellationToken);

            if (defaultAddresses == null)
            {
                _logger.LogWarning("No default addresses found for user {UserId}", userId);
                return Result.Ok();
            }
            defaultAddresses.UnsetAsDefault();
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error unsetting default addresses for user {UserId}", userId);
            return Result.Failure(new PersistenceError(e.Message));
        }
      
    }

    public async Task<Result> SetDefaultAddressAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var result = await UnsetAllDefaultsForUserAsync(userId, cancellationToken);
        if(result.IsFailure) return result;
        
        try
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken: cancellationToken);

            if (address == null)
            {
                _logger.LogWarning("Address {AddressId} not found for user {UserId} when setting default", addressId, userId);
                return Result.Failure(new NotFoundError("Address", addressId.ToString()));
            }

            address.SetAsDefault();
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Ok();
          
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting address {AddressId} as default for user {UserId}", addressId, userId);
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(new PersistenceError(e.Message));
        }
        
    }
}
