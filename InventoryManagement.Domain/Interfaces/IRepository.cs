using InventoryManagement.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // Read operations
        Task<T> GetByIdAsync(long id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> spec);
        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Write operations
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);

        // Async write operations (for special cases)
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}
