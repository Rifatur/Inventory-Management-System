using AutoMapper;
using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Inventory;
using InventoryManagement.Application.DTOs.Order;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Services
{
    /// <summary>
    /// Implementation of order management operations
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly IIntegrationService _integrationService;

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<OrderService> logger,
            IInventoryService inventoryService,
            IIntegrationService integrationService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _inventoryService = inventoryService;
            _integrationService = integrationService;
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto orderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generate order number
                var orderNumber = GenerateOrderNumber();

                // Create order
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = orderDto.CustomerId,
                    CustomerName = orderDto.CustomerName,
                    CustomerEmail = orderDto.CustomerEmail,
                    Status = "Pending",
                    OrderDate = DateTime.Now,
                    ShippingAddress = orderDto.ShippingAddress,
                    BillingAddress = orderDto.BillingAddress ?? orderDto.ShippingAddress,
                    PaymentMethod = orderDto.PaymentMethod,
                    PaymentStatus = "Pending",
                    Notes = orderDto.Notes,
                    OrderSource = orderDto.OrderSource ?? "Web",
                    CreatedAt = DateTime.Now
                };

                // Calculate totals
                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<OrderDto>
                        {
                            Success = false,
                            Message = $"Product with ID {itemDto.ProductId} not found"
                        };
                    }

                    var lineTotal = (product.UnitPrice * itemDto.Quantity) - itemDto.DiscountAmount;
                    subTotal += lineTotal;

                    var orderItem = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        SKU = product.SKU,
                        ProductName = product.Name,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount,
                        LineTotal = lineTotal,
                        FulfillmentStatus = "Pending"
                    };

                    orderItems.Add(orderItem);
                }

                // Calculate tax and shipping
                var taxRate = 0.08m; // 8% tax rate - should be configurable
                var taxAmount = subTotal * taxRate;
                var shippingCost = CalculateShippingCost(orderItems);

                order.SubTotal = subTotal;
                order.TaxAmount = taxAmount;
                order.ShippingCost = shippingCost;
                order.TotalAmount = subTotal + taxAmount + shippingCost;
                order.OrderItems = orderItems;

                // Save order
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Reserve inventory
                var reservationResult = await _inventoryService.ReserveInventoryAsync(
                    order.OrderId,
                    orderDto.OrderItems);

                if (!reservationResult.Success)
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = reservationResult.Message,
                        Errors = reservationResult.Errors
                    };
                }

                // Assign warehouse
                await AssignOptimalWarehouse(order);

                await transaction.CommitAsync();

                // Notify external systems
                await _integrationService.UpdateOrderStatusInExternalSystemAsync(
                    order.OrderId, order.Status, orderDto.OrderSource);

                var resultDto = _mapper.Map<OrderDto>(order);

                _logger.LogInformation("Created order {OrderNumber} with {ItemCount} items",
                    order.OrderNumber, order.OrderItems.Count);

                return new ApiResponse<OrderDto>
                {
                    Success = true,
                    Message = "Order created successfully",
                    Data = resultDto
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order");

                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(long orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.AssignedWarehouse)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                var orderDto = _mapper.Map<OrderDto>(order);

                return new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = orderDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(long orderId, string newStatus, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = false
                    };
                }

                var oldStatus = order.Status;
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Handle status-specific logic
                switch (newStatus)
                {
                    case "Processing":
                        // Ensure inventory is reserved
                        break;

                    case "Shipped":
                        // Update fulfillment status
                        foreach (var item in order.OrderItems)
                        {
                            item.FulfillmentStatus = "Shipped";
                        }
                        break;

                    case "Delivered":
                        order.DeliveredDate = DateTime.UtcNow;
                        order.PaymentStatus = "Paid";
                        break;

                    case "Cancelled":
                        // Release inventory reservations
                        await _inventoryService.ReleaseReservationAsync(orderId);
                        break;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Notify external systems
                await _integrationService.UpdateOrderStatusInExternalSystemAsync(
                    orderId, newStatus, order.OrderSource);

                _logger.LogInformation("Updated order {OrderId} status from {OldStatus} to {NewStatus}",
                    orderId, oldStatus, newStatus);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating order status");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating order status",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Helper methods

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private decimal CalculateShippingCost(List<OrderItem> items)
        {
            // Simple shipping calculation - should be more sophisticated in production
            var totalWeight = items.Sum(i => i.Quantity * 0.5m); // Assuming 0.5kg average weight
            return totalWeight * 2.5m; // $2.50 per kg
        }

        private async Task AssignOptimalWarehouse(Order order)
        {
            // Simple warehouse assignment - assign to warehouse with most inventory
            // In production, consider location, capacity, etc.
            var warehouseStock = await _context.InventoryItems
                .Where(i => order.OrderItems.Select(oi => oi.ProductId).Contains(i.ProductId))
                .GroupBy(i => i.WarehouseId).Select(g => new
                {
                    WarehouseId = g.Key,
                    TotalAvailable = g.Sum(i => i.QuantityAvailable)
                })
                .OrderByDescending(w => w.TotalAvailable)
                .FirstOrDefaultAsync();

            if (warehouseStock != null)
            {
                order.AssignedWarehouseId = warehouseStock.WarehouseId;

                // Update order items with fulfillment warehouse
                foreach (var item in order.OrderItems)
                {
                    item.FulfilledFromWarehouseId = warehouseStock.WarehouseId;
                }
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.AssignedWarehouse)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                {
                    return new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                var orderDto = _mapper.Map<OrderDto>(order);

                return new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = orderDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order by number {OrderNumber}", orderNumber);
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the order",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PaginatedResponse<OrderDto>>> GetOrdersAsync(
            string customerId, string status, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.AssignedWarehouse)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    query = query.Where(o => o.CustomerId == customerId);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= toDate.Value);
                }

                var totalCount = await query.CountAsync();

                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var orderDtos = _mapper.Map<List<OrderDto>>(orders);

                var response = new PaginatedResponse<OrderDto>
                {
                    Items = orderDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return new ApiResponse<PaginatedResponse<OrderDto>>
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return new ApiResponse<PaginatedResponse<OrderDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving orders",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(long orderId, string reason, string userId)
        {
            return await UpdateOrderStatusAsync(orderId, "Cancelled", userId);
        }

        public async Task<ApiResponse<bool>> AssignWarehouseAsync(long orderId, long warehouseId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = false
                    };
                }

                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                if (warehouse == null || !warehouse.IsActive)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid or inactive warehouse",
                        Data = false
                    };
                }

                order.AssignedWarehouseId = warehouseId;
                order.UpdatedAt = DateTime.UtcNow;

                // Update order items
                foreach (var item in order.OrderItems)
                {
                    item.FulfilledFromWarehouseId = warehouseId;
                }

                await _context.SaveChangesAsync();

                // Notify warehouse system
                await _integrationService.NotifyWarehouseSystemAsync(orderId, warehouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Warehouse assigned successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning warehouse to order {OrderId}", orderId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while assigning warehouse",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateTrackingNumberAsync(long orderId, string trackingNumber)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = false
                    };
                }

                order.TrackingNumber = trackingNumber;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Tracking number updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking number for order {OrderId}", orderId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating tracking number",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ProcessOrderFulfillmentAsync(long orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = false
                    };
                }

                if (order.Status != "Processing")
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order must be in Processing status to fulfill",
                        Data = false
                    };
                }

                // Process each order item
                foreach (var item in order.OrderItems)
                {
                    // Deduct from inventory
                    var updateDto = new UpdateInventoryDto
                    {
                        ProductId = item.ProductId,
                        WarehouseId = item.FulfilledFromWarehouseId ?? order.AssignedWarehouseId ?? 0,
                        Quantity = -item.Quantity,
                        MovementType = "Shipment",
                        ReferenceNumber = order.OrderNumber
                    };

                    var result = await _inventoryService.UpdateInventoryAsync(updateDto, "System");

                    if (!result.Success)
                    {
                        await transaction.RollbackAsync();
                        return result;
                    }

                    item.FulfillmentStatus = "Picked";
                }

                // Update order status
                order.Status = "ReadyToShip";
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order fulfillment processed successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing order fulfillment for order {OrderId}", orderId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while processing order fulfillment",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }


    }
}
