using InventoryManagement.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern for managing transactions across repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IProductRepository Products { get; }
        IInventoryRepository Inventory { get; }
        IOrderRepository Orders { get; }
        IWarehouseRepository Warehouses { get; }
        IStockMovementRepository StockMovements { get; }

        // Generic repository factory
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // Transaction management
        Task<int> SaveChangesAsync();
        Task<bool> SaveEntitiesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Database operations
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
        Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql);
    }
}
