using AutoMapper;
using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryManagement.Application.Services
{
    /// <summary>
    /// Service implementation for warehouse management operations
    /// </summary>
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehouseService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IIntegrationService _integrationService;

        public WarehouseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<WarehouseService> logger,
            IDistributedCache cache,
            IIntegrationService integrationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _integrationService = integrationService;
        }

        /// <summary>
        /// Creates a new warehouse
        /// </summary>
        public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto warehouseDto)
        {
            try
            {
                // Validate warehouse code uniqueness
                var existingWarehouse = await _unitOfWork.Warehouses
                    .GetWarehouseByCodeAsync(warehouseDto.WarehouseCode);

                if (existingWarehouse != null)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = "Warehouse with this code already exists",
                        Errors = new List<string> { "Duplicate warehouse code" }
                    };
                }

                // Validate capacity
                if (warehouseDto.MaxCapacity <= 0)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = "Maximum capacity must be greater than zero",
                        Errors = new List<string> { "Invalid capacity value" }
                    };
                }

                // Create warehouse entity
                var warehouse = _mapper.Map<Warehouse>(warehouseDto);
                warehouse.CreatedAt = DateTime.UtcNow;
                warehouse.CurrentUtilization = 0; // New warehouse starts empty
                warehouse.IsActive = true;

                // Add to repository
                _unitOfWork.Warehouses.Add(warehouse);
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                await ClearWarehouseCacheAsync();

                // Map to DTO
                var resultDto = _mapper.Map<WarehouseDto>(warehouse);

                // Notify external systems
                await _integrationService.NotifyWarehouseSystemAsync(0, warehouse.WarehouseId);

                _logger.LogInformation("Created new warehouse: {WarehouseCode} with ID: {WarehouseId}",
                    warehouse.WarehouseCode, warehouse.WarehouseId);

                return new ApiResponse<WarehouseDto>
                {
                    Success = true,
                    Message = "Warehouse created successfully",
                    Data = resultDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse with code: {WarehouseCode}",
                    warehouseDto.WarehouseCode);

                return new ApiResponse<WarehouseDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the warehouse",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Updates an existing warehouse
        /// </summary>
        public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(UpdateWarehouseDto warehouseDto)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseDto.WarehouseId);

                if (warehouse == null)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = "Warehouse not found"
                    };
                }

                // Check if warehouse code is being changed
                if (warehouse.WarehouseCode != warehouseDto.WarehouseCode)
                {
                    var existingWarehouse = await _unitOfWork.Warehouses
                        .GetWarehouseByCodeAsync(warehouseDto.WarehouseCode);

                    if (existingWarehouse != null && existingWarehouse.WarehouseId != warehouseDto.WarehouseId)
                    {
                        return new ApiResponse<WarehouseDto>
                        {
                            Success = false,
                            Message = "Another warehouse with this code already exists"
                        };
                    }
                }

                // Validate capacity change
                if (warehouseDto.MaxCapacity < warehouse.CurrentUtilization)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = $"Cannot reduce capacity below current utilization ({warehouse.CurrentUtilization})"
                    };
                }

                // Store old values for audit
                var oldValues = JsonSerializer.Serialize(warehouse);

                // Update warehouse
                _mapper.Map(warehouseDto, warehouse);
                warehouse.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Warehouses.Update(warehouse);
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                await ClearWarehouseCacheAsync();
                await InvalidateWarehouseCacheAsync(warehouse.WarehouseId);

                var resultDto = _mapper.Map<WarehouseDto>(warehouse);

                _logger.LogInformation("Updated warehouse: {WarehouseCode} (ID: {WarehouseId})",
                    warehouse.WarehouseCode, warehouse.WarehouseId);

                return new ApiResponse<WarehouseDto>
                {
                    Success = true,
                    Message = "Warehouse updated successfully",
                    Data = resultDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse {WarehouseId}", warehouseDto.WarehouseId);

                return new ApiResponse<WarehouseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the warehouse",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Gets a warehouse by ID
        /// </summary>
        public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(int warehouseId)
        {
            try
            {
                // Check cache first
                var cacheKey = $"warehouse:{warehouseId}";
                var cachedWarehouse = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedWarehouse))
                {
                    var cachedDto = JsonSerializer.Deserialize<WarehouseDto>(cachedWarehouse);
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = true,
                        Data = cachedDto
                    };
                }

                // Get from database
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);

                if (warehouse == null)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = "Warehouse not found"
                    };
                }

                var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);

                // Cache the result
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(warehouseDto),
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(15)
                    });

                return new ApiResponse<WarehouseDto>
                {
                    Success = true,
                    Data = warehouseDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouse {WarehouseId}", warehouseId);

                return new ApiResponse<WarehouseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the warehouse",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        public async Task<ApiResponse<List<WarehouseDto>>> GetAllWarehousesAsync(bool activeOnly = true)
        {
            try
            {
                // Check cache
                var cacheKey = $"warehouses:all:{activeOnly}";
                var cachedWarehouses = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedWarehouses))
                {
                    var cachedList = JsonSerializer.Deserialize<List<WarehouseDto>>(cachedWarehouses);
                    return new ApiResponse<List<WarehouseDto>>
                    {
                        Success = true,
                        Data = cachedList
                    };
                }

                // Get from database
                var warehouses = activeOnly
                    ? await _unitOfWork.Warehouses.GetActiveWarehousesAsync()
                    : await _unitOfWork.Warehouses.GetAllAsync();

                var warehouseDtos = _mapper.Map<List<WarehouseDto>>(warehouses);

                // Cache the result
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(warehouseDtos),
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });

                return new ApiResponse<List<WarehouseDto>>
                {
                    Success = true,
                    Data = warehouseDtos,
                    Message = $"Found {warehouseDtos.Count} warehouses"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses");

                return new ApiResponse<List<WarehouseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving warehouses",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Gets warehouse summary with statistics
        /// </summary>
        public async Task<ApiResponse<WarehouseSummaryDto>> GetWarehouseSummaryAsync(int warehouseId)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);

                if (warehouse == null)
                {
                    return new ApiResponse<WarehouseSummaryDto>
                    {
                        Success = false,
                        Message = "Warehouse not found"
                    };
                }

                // Get inventory statistics
                var inventoryItems = await _unitOfWork.Inventory.GetInventoryByWarehouseAsync(warehouseId);

                // Get recent movements
                var recentMovements = await _unitOfWork.StockMovements
                    .GetMovementsByWarehouseAsync(warehouseId, DateTime.UtcNow.AddDays(-30));

                // Calculate statistics
                var summary = new WarehouseSummaryDto
                {
                    WarehouseId = warehouse.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.Name,
                    WarehouseType = warehouse.WarehouseType,
                    Address = warehouse.Address,
                    City = warehouse.City,
                    State = warehouse.State,
                    Country = warehouse.Country,
                    IsActive = warehouse.IsActive,
                    MaxCapacity = warehouse.MaxCapacity,
                    CurrentUtilization = warehouse.CurrentUtilization,
                    UtilizationPercentage = (warehouse.CurrentUtilization / (decimal)warehouse.MaxCapacity) * 100,

                    // Inventory statistics
                    TotalProducts = inventoryItems.Select(i => i.ProductId).Distinct().Count(),
                    TotalQuantity = inventoryItems.Sum(i => i.QuantityOnHand),
                    TotalValue = inventoryItems.Sum(i => i.QuantityOnHand * i.Product.UnitCost),
                    TotalReserved = inventoryItems.Sum(i => i.QuantityReserved),

                    // Movement statistics (last 30 days)
                    InboundMovements = recentMovements.Count(m => m.ToWarehouseId == warehouseId),
                    OutboundMovements = recentMovements.Count(m => m.FromWarehouseId == warehouseId),
                    TotalMovementValue = recentMovements.Sum(m => m.TotalCost),

                    // Top products by quantity
                    TopProductsByQuantity = inventoryItems
                        .OrderByDescending(i => i.QuantityOnHand)
                        .Take(10)
                        .Select(i => new ProductStockInfo
                        {
                            ProductId = i.ProductId,
                            SKU = i.Product.SKU,
                            ProductName = i.Product.Name,
                            QuantityOnHand = i.QuantityOnHand,
                            QuantityReserved = i.QuantityReserved,
                            Value = i.QuantityOnHand * i.Product.UnitCost
                        })
                        .ToList(),

                    // Low stock items
                    LowStockItems = inventoryItems
                        .Where(i => i.QuantityOnHand <= i.Product.ReorderLevel)
                        .Select(i => new ProductStockInfo
                        {
                            ProductId = i.ProductId,
                            SKU = i.Product.SKU,
                            ProductName = i.Product.Name,
                            QuantityOnHand = i.QuantityOnHand,
                            ReorderLevel = i.Product.ReorderLevel,
                            ReorderQuantity = i.Product.ReorderQuantity
                        })
                        .ToList(),

                    LastUpdated = DateTime.UtcNow
                };

                return new ApiResponse<WarehouseSummaryDto>
                {
                    Success = true,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating warehouse summary for {WarehouseId}", warehouseId);

                return new ApiResponse<WarehouseSummaryDto>
                {
                    Success = false,
                    Message = "An error occurred while generating warehouse summary",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Deactivates a warehouse
        /// </summary>
        public async Task<ApiResponse<bool>> DeactivateWarehouseAsync(int warehouseId, string userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);

                if (warehouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Warehouse not found",
                        Data = false
                    };
                }

                if (!warehouse.IsActive)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Warehouse is already inactive",
                        Data = true
                    };
                }

                // Check if warehouse has inventory
                var inventoryItems = await _unitOfWork.Inventory.GetInventoryByWarehouseAsync(warehouseId);
                var hasInventory = inventoryItems.Any(i => i.QuantityOnHand > 0);

                if (hasInventory)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot deactivate warehouse with existing inventory. Please transfer all inventory first.",
                        Data = false
                    };
                }

                // Check for pending orders
                var pendingOrders = await _unitOfWork.Orders.GetOrdersByStatusAsync("Pending");
                var hasAssignedOrders = pendingOrders.Any(o => o.AssignedWarehouseId == warehouseId);

                if (hasAssignedOrders)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot deactivate warehouse with pending orders. Please reassign orders first.",
                        Data = false
                    };
                }

                // Deactivate warehouse
                warehouse.IsActive = false;
                warehouse.UpdatedAt = DateTime.UtcNow;
                warehouse.UpdatedBy = userId;

                _unitOfWork.Warehouses.Update(warehouse);
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                await ClearWarehouseCacheAsync();
                await InvalidateWarehouseCacheAsync(warehouseId);

                // Log audit
                _logger.LogInformation("Warehouse {WarehouseCode} (ID: {WarehouseId}) deactivated by {UserId}",
                    warehouse.WarehouseCode, warehouse.WarehouseId, userId);

                await _unitOfWork.CommitTransactionAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Warehouse deactivated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deactivating warehouse {WarehouseId}", warehouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deactivating the warehouse",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Additional service methods

        /// <summary>
        /// Gets warehouses by type
        /// </summary>
        public async Task<ApiResponse<List<WarehouseDto>>> GetWarehousesByTypeAsync(string warehouseType)
        {
            try
            {
                var warehouses = await _unitOfWork.Warehouses.GetWarehousesByTypeAsync(warehouseType);
                var warehouseDtos = _mapper.Map<List<WarehouseDto>>(warehouses);

                return new ApiResponse<List<WarehouseDto>>
                {
                    Success = true,
                    Data = warehouseDtos,
                    Message = $"Found {warehouseDtos.Count} {warehouseType} warehouses"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses by type: {WarehouseType}", warehouseType);

                return new ApiResponse<List<WarehouseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving warehouses",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Gets nearest warehouse based on postal code
        /// </summary>
        public async Task<ApiResponse<WarehouseDto>> GetNearestWarehouseAsync(string postalCode)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetNearestWarehouseAsync(postalCode);

                if (warehouse == null)
                {
                    return new ApiResponse<WarehouseDto>
                    {
                        Success = false,
                        Message = "No active warehouse found for the specified location"
                    };
                }

                var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);

                return new ApiResponse<WarehouseDto>
                {
                    Success = true,
                    Data = warehouseDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearest warehouse for postal code: {PostalCode}", postalCode);

                return new ApiResponse<WarehouseDto>
                {
                    Success = false,
                    Message = "An error occurred while finding nearest warehouse",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Updates warehouse utilization
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateWarehouseUtilizationAsync(int warehouseId)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);

                if (warehouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Warehouse not found",
                        Data = false
                    };
                }

                // Calculate current utilization based on inventory
                var inventoryItems = await _unitOfWork.Inventory.GetInventoryByWarehouseAsync(warehouseId);

                // This is a simplified calculation - in reality, you'd consider volume/space
                var totalItems = inventoryItems.Sum(i => i.QuantityOnHand);
                var utilizationPercentage = (totalItems / (decimal)warehouse.MaxCapacity) * 100;

                warehouse.CurrentUtilization = Math.Min(utilizationPercentage, 100); // Cap at 100%
                warehouse.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Warehouses.Update(warehouse);
                await _unitOfWork.SaveChangesAsync();

                // Update cache
                await InvalidateWarehouseCacheAsync(warehouseId);

                _logger.LogInformation("Updated utilization for warehouse {WarehouseId} to {Utilization}%",
                    warehouseId, warehouse.CurrentUtilization);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Warehouse utilization updated to {warehouse.CurrentUtilization:F2}%",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse utilization for {WarehouseId}", warehouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating warehouse utilization",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Gets warehouse capacity report
        /// </summary>
        public async Task<ApiResponse<List<WarehouseCapacityDto>>> GetWarehouseCapacityReportAsync()
        {
            try
            {
                var warehouses = await _unitOfWork.Warehouses.GetActiveWarehousesAsync();
                var utilizationData = await _unitOfWork.Warehouses.GetWarehouseUtilizationAsync();

                var capacityReport = warehouses.Select(w => new WarehouseCapacityDto
                {
                    WarehouseId = w.WarehouseId,
                    WarehouseCode = w.WarehouseCode,
                    WarehouseName = w.Name,
                    WarehouseType = w.WarehouseType,
                    MaxCapacity = w.MaxCapacity,
                    CurrentUtilization = utilizationData.ContainsKey(w.WarehouseId)
                        ? utilizationData[w.WarehouseId]
                        : w.CurrentUtilization,
                    AvailableCapacity = w.MaxCapacity - (int)(w.MaxCapacity * (w.CurrentUtilization / 100)),
                    UtilizationPercentage = w.CurrentUtilization,
                    Status = w.CurrentUtilization switch
                    {
                        >= 90 => "Critical",
                        >= 75 => "High",
                        >= 50 => "Medium",
                        _ => "Low"
                    }
                }).OrderByDescending(w => w.UtilizationPercentage).ToList();

                return new ApiResponse<List<WarehouseCapacityDto>>
                {
                    Success = true,
                    Data = capacityReport,
                    Message = "Warehouse capacity report generated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating warehouse capacity report");

                return new ApiResponse<List<WarehouseCapacityDto>>
                {
                    Success = false,
                    Message = "An error occurred while generating capacity report",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Transfers all inventory from one warehouse to another
        /// </summary>
        public async Task<ApiResponse<bool>> TransferAllInventoryAsync(
            int fromWarehouseId,
            int toWarehouseId,
            string reason,
            string userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Validate warehouses
                var fromWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(fromWarehouseId);
                var toWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(toWarehouseId);

                if (fromWarehouse == null || toWarehouse == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "One or both warehouses not found",
                        Data = false
                    };
                }

                if (!toWarehouse.IsActive)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Destination warehouse is not active",
                        Data = false
                    };
                }

                // Get all inventory from source warehouse
                var inventoryItems = await _unitOfWork.Inventory.GetInventoryByWarehouseAsync(fromWarehouseId);
                var itemsToTransfer = inventoryItems.Where(i => i.QuantityOnHand > 0).ToList();

                if (!itemsToTransfer.Any())
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "No inventory to transfer",
                        Data = true
                    };
                }

                // Check destination warehouse capacity
                var totalItemsToTransfer = itemsToTransfer.Sum(i => i.QuantityOnHand);
                var destinationAvailableCapacity = toWarehouse.MaxCapacity -
                    (int)(toWarehouse.MaxCapacity * (toWarehouse.CurrentUtilization / 100));

                if (totalItemsToTransfer > destinationAvailableCapacity)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Insufficient capacity in destination warehouse. " +
                                 $"Required: {totalItemsToTransfer}, Available: {destinationAvailableCapacity}",
                        Data = false
                    };
                }

                // Process transfers
                foreach (var item in itemsToTransfer)
                {
                    // Create or update destination inventory
                    var destInventory = await _unitOfWork.Inventory
                        .GetInventoryAsync(item.ProductId, toWarehouseId);

                    if (destInventory == null)
                    {
                        destInventory = new InventoryItem
                        {
                            ProductId = item.ProductId,
                            WarehouseId = toWarehouseId,
                            QuantityOnHand = 0,
                            QuantityReserved = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        _unitOfWork.Inventory.Add(destInventory);
                    }

                    // Update quantities
                    destInventory.QuantityOnHand += item.QuantityOnHand;
                    destInventory.LastUpdated = DateTime.UtcNow;

                    // Create stock movement record
                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        FromWarehouseId = fromWarehouseId,
                        ToWarehouseId = toWarehouseId,
                        MovementType = "Transfer",
                        Quantity = item.QuantityOnHand,
                        ReferenceNumber = $"BULK-TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
                        Reason = reason,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UnitCost = item.Product.UnitCost,
                        TotalCost = item.QuantityOnHand * item.Product.UnitCost
                    };
                    _unitOfWork.StockMovements.Add(movement);

                    // Clear source inventory
                    item.QuantityOnHand = 0;
                    item.LastUpdated = DateTime.UtcNow;
                    _unitOfWork.Inventory.Update(item);
                }

                await _unitOfWork.SaveChangesAsync();

                // Update warehouse utilizations
                await UpdateWarehouseUtilizationAsync(fromWarehouseId);
                await UpdateWarehouseUtilizationAsync(toWarehouseId);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Transferred all inventory from warehouse {FromId} to {ToId}. " +
                                     "Items transferred: {Count}, Total quantity: {Quantity}",
                    fromWarehouseId, toWarehouseId, itemsToTransfer.Count, totalItemsToTransfer);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Successfully transferred {itemsToTransfer.Count} products " +
                             $"with total quantity of {totalItemsToTransfer} items",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error transferring inventory from warehouse {FromId} to {ToId}",
                    fromWarehouseId, toWarehouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while transferring inventory",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Cache management methods

        private async Task ClearWarehouseCacheAsync()
        {
            var cacheKeys = new[]
            {
                "warehouses:all:true",
                "warehouses:all:false"
            };

            foreach (var key in cacheKeys)
            {
                await _cache.RemoveAsync(key);
            }
        }

        private async Task InvalidateWarehouseCacheAsync(int warehouseId)
        {
            await _cache.RemoveAsync($"warehouse:{warehouseId}");
        }


    }
}
