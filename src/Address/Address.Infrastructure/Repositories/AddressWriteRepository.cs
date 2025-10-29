using Microsoft.EntityFrameworkCore;
using Address.Core.Domain;
using Address.Core.Repositories;
using Address.Infrastructure.EF;

namespace Address.Infrastructure.Repositories;

public class AddressWriteRepository : IAddressWriteRepository
{
    private readonly AddressDbContext _context;

    public AddressWriteRepository(AddressDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AddressEntity> AddAsync(AddressEntity address, CancellationToken cancellationToken = default)
    {
        await _context.Addresses.AddAsync(address, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return address;
    }

    public async Task<AddressEntity> UpdateAsync(AddressEntity address, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync(cancellationToken);
        return address;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses.FindAsync(new object[] { id }, cancellationToken);
        if (address == null)
        {
            return false;
        }

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task UnsetAllDefaultsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var defaultAddresses = await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var address in defaultAddresses)
        {
            address.UnsetAsDefault();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
