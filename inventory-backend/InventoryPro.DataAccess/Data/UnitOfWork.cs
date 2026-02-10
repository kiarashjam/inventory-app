using InventoryPro.Domain.Interfaces;

namespace InventoryPro.DataAccess.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly InventoryProDbContext _context;

    public UnitOfWork(InventoryProDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
