using InventoryManagement.Domain.Interfaces.Repositories;
using InventoryManagement.Domain.longerfaces.Repositories;
using Microsoft.EntityFrameworkCore;
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

        // Stored procedure execution
        Task<List<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, params object[] parameters) where T : class;

        //public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, params object[] parameters) where T : class
        //{
        //    // Build SQL command
        //    var sql = $"EXEC {storedProcedureName} {string.Join(", ", parameters.Select((_, i) => $"@p{i}"))}";

        //    // Execute and return results
        //    return await _context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        //}
        //var topProducts = await unitOfWork.ExecuteStoredProcedureAsync<Product>("GetTopSellingProducts");
        //var topProducts = await unitOfWork.ExecuteStoredProcedureAsync<Product>(
        //    "GetTopSellingProductsByDate",
        //    new SqlParameter("@StartDate", startDate),
        //    new SqlParameter("@EndDate", endDate));
    }
}
