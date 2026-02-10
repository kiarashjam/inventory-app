namespace InventoryPro.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveAsync();
}
