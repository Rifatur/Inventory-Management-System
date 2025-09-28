using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for order management operations
    /// </summary>
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto orderDto);
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(long orderId);
        Task<ApiResponse<OrderDto>> GetOrderByNumberAsync(string orderNumber);
        Task<ApiResponse<PaginatedResponse<OrderDto>>> GetOrdersAsync(string customerId, string status, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(long orderId, string newStatus, string userId);
        Task<ApiResponse<bool>> CancelOrderAsync(long orderId, string reason, string userId);
        Task<ApiResponse<bool>> AssignWarehouseAsync(long orderId, long warehouseId);
        Task<ApiResponse<bool>> UpdateTrackingNumberAsync(long orderId, string trackingNumber);
        Task<ApiResponse<bool>> ProcessOrderFulfillmentAsync(long orderId);
    }
}
