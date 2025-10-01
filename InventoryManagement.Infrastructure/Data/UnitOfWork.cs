using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Domain.Interfaces.Repositories;
using InventoryManagement.Domain.longerfaces.Repositories;
using InventoryManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace InventoryManagement.Infrastructure.Data
{
    /// <summary>
    /// Unit of Work implementation for managing transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _currentTransaction;
        private Hashtable _repositories;

        // Repository instances
        private IProductRepository _productRepository;
        private IInventoryRepository _inventoryRepository;
        private IOrderRepository _orderRepository;
        private IWarehouseRepository _warehouseRepository;
        private IStockMovementRepository _stockMovementRepository;

        public IProductRepository Products => throw new NotImplementedException();

        public IInventoryRepository Inventory => throw new NotImplementedException();

        public IOrderRepository Orders => throw new NotImplementedException();

        public IWarehouseRepository Warehouses => throw new NotImplementedException();

        public IStockMovementRepository StockMovements => throw new NotImplementedException();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new Exception("A concurrency conflict occurred. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                throw new Exception("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task<bool> SaveEntitiesAsync()
        {
            return await SaveChangesAsync() > 0;
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _currentTransaction?.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql)
        {
            return await _context.Database.ExecuteSqlInterpolatedAsync(sql);
        }

        public void Dispose()
        {
            _context?.Dispose();
            _currentTransaction?.Dispose();
        }

        public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string storedProcedureName, params object[] parameters) where T : class
        {
            var sql = $"EXEC {storedProcedureName} {string.Join(", ", parameters.Select((_, i) => $"@p{i}"))}";
            // Execute and return results
            return await _context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }
    }
}
