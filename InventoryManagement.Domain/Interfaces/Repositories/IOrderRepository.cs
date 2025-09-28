using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Order-specific repository interface
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order> GetOrderWithItemsAsync(long orderId);
        Task<Order> GetOrderByNumberAsync(string orderNumber);
        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(string customerId);
        Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status);
        Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> GenerateOrderNumberAsync();
        Task<decimal> GetCustomerTotalOrderValueAsync(string customerId);
        Task<Dictionary<string, int>> GetOrderCountByStatusAsync();
    }
}
