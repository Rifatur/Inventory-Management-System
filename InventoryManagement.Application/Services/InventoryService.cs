using AutoMapper;
using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Inventory;
using InventoryManagement.Application.DTOs.Order;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Stock;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;


namespace InventoryManagement.Application.Services
{
    /// <summary>
    /// Inventory service implementation using Repository Pattern and Unit of Work
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IIntegrationService _integrationService;

        public InventoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<InventoryService> logger,
            IDistributedCache cache,
            IIntegrationService integrationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _integrationService = integrationService;
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto productDto, string userId)
        {
            try
            {
                // Check SKU uniqueness using repository
                var isUnique = await _unitOfWork.Products.IsSkuUniqueAsync(productDto.SKU);

                if (!isUnique)
                {
                    return new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Product with this SKU already exists",
                        Errors = new List<string> { "Duplicate SKU" }
                    };
                }

                // Create product entity
                var product = _mapper.Map<Product>(productDto);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedBy = userId;

                // Add using repository
                _unitOfWork.Products.Add(product);

                // Save changes using Unit of Work
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                await ClearProductCacheAsync();

                // Map to DTO
                var resultDto = _mapper.Map<ProductDto>(product);

                // Notify external systems
                await _integrationService.PushInventoryLevelsToERPAsync();

                return new ApiResponse<ProductDto>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = resultDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with SKU: {SKU}", productDto.SKU);
                return new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateInventoryAsync(UpdateInventoryDto updateDto, string userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Get or create inventory item using repository
                var inventoryItem = await _unitOfWork.Inventory.GetInventoryAsync(
                    updateDto.ProductId, updateDto.WarehouseId);

                if (inventoryItem == null)
                {
                    // Get product to ensure it exists
                    var product = await _unitOfWork.Products.GetByIdAsync(updateDto.ProductId);
                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "Product not found",
                            Data = false
                        };
                    }

                    // Create new inventory item
                    inventoryItem = new InventoryItem
                    {
                        ProductId = updateDto.ProductId,
                        WarehouseId = updateDto.WarehouseId,
                        QuantityOnHand = 0,
                        QuantityReserved = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _unitOfWork.Inventory.Add(inventoryItem);
                }

                // Calculate new quantity
                var oldQuantity = inventoryItem.QuantityOnHand;
                var newQuantity = oldQuantity + updateDto.Quantity;

                // Validate quantity
                if (newQuantity < 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Insufficient inventory. Cannot reduce quantity below zero.",
                        Data = false
                    };
                }

                // Update inventory
                inventoryItem.QuantityOnHand = newQuantity;
                inventoryItem.LastUpdated = DateTime.UtcNow;
                _unitOfWork.Inventory.Update(inventoryItem);

                // Create stock movement record
                var stockMovement = new StockMovement
                {
                    ProductId = updateDto.ProductId,
                    ToWarehouseId = updateDto.WarehouseId,
                    MovementType = updateDto.MovementType,
                    Quantity = updateDto.Quantity,
                    ReferenceNumber = updateDto.ReferenceNumber,
                    Reason = updateDto.Reason,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UnitCost = updateDto.UnitCost ?? inventoryItem.Product.UnitCost,
                    TotalCost = (updateDto.UnitCost ?? inventoryItem.Product.UnitCost) * Math.Abs(updateDto.Quantity)
                };

                _unitOfWork.StockMovements.Add(stockMovement);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Update cache
                await UpdateInventoryCacheAsync(updateDto.ProductId, updateDto.WarehouseId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Inventory updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating inventory for Product: {ProductId}, Warehouse: {WarehouseId}",
                    updateDto.ProductId, updateDto.WarehouseId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating inventory",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(InventorySearchDto searchDto)
        {
            try
            {
                // Create specification
                var spec = new ProductsBySearchSpecification(
                    searchDto.SearchTerm,
                    searchDto.Category,
                    searchDto.LowStockOnly,
                    searchDto.PageNumber,
                    searchDto.PageSize);

                // Get products using specification
                var products = await _unitOfWork.Products.GetAsync(spec);
                var totalCount = await _unitOfWork.Products.CountAsync(spec);

                // Map to DTOs
                var productDtos = new List<ProductDto>();
                foreach (var product in products)
                {
                    var dto = _mapper.Map<ProductDto>(product);

                    // Get stock levels
                    dto.TotalQuantityOnHand = await _unitOfWork.Inventory.GetTotalQuantityAsync(product.ProductId);
                    dto.TotalQuantityAvailable = dto.TotalQuantityOnHand -
                        product.InventoryItems.Sum(i => i.QuantityReserved);

                    productDtos.Add(dto);
                }

                var response = new PaginatedResponse<ProductDto>
                {
                    Items = productDtos,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize)
                };

                return new ApiResponse<PaginatedResponse<ProductDto>>
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return new ApiResponse<PaginatedResponse<ProductDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> ReserveInventoryAsync(long orderId, List<CreateOrderItemDto> items)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    // Check availability
                    var isAvailable = await _unitOfWork.Inventory.CheckAvailabilityAsync(
                        item.ProductId, item.Quantity);

                    if (!isAvailable)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        var totalAvailable = await _unitOfWork.Inventory.GetTotalQuantityAsync(item.ProductId);

                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = $"Insufficient inventory for product ID {item.ProductId}. " +
                                     $"Requested: {item.Quantity}, Available: {totalAvailable}",
                            Data = false
                        };
                    }

                    // Find best warehouse for reservation
                    var inventoryItems = await _unitOfWork.Inventory.GetInventoryByProductAsync(item.ProductId);
                    var availableInventory = inventoryItems
                        .Where(i => i.QuantityAvailable >= item.Quantity && i.Warehouse.IsActive)
                        .OrderBy(i => i.Warehouse.WarehouseType == "Main" ? 0 : 1)
                        .ThenBy(i => i.WarehouseId)
                        .FirstOrDefault();

                    if (availableInventory == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = $"No single warehouse has sufficient inventory for product ID {item.ProductId}",
                            Data = false
                        };
                    }

                    // Reserve stock
                    var reserved = await _unitOfWork.Inventory.ReserveStockAsync(
                        item.ProductId,
                        availableInventory.WarehouseId,
                        item.Quantity,
                        orderId);

                    if (!reserved)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "Failed to reserve inventory",
                            Data = false
                        };
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Reserved inventory for order {OrderId} with {ItemCount} items",
                    orderId, items.Count);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Inventory reserved successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error reserving inventory for order {OrderId}", orderId);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while reserving inventory",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Helper method implementations
        private async Task UpdateInventoryCacheAsync(long productId, long warehouseId)
        {
            var cacheKey = $"inventory:{productId}:{warehouseId}";
            var inventory = await _unitOfWork.Inventory.GetInventoryAsync(productId, warehouseId);

            if (inventory != null)
            {
                var cacheData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    inventory.QuantityOnHand,
                    inventory.QuantityReserved,
                    inventory.QuantityAvailable,
                    inventory.LastUpdated
                });

                await _cache.SetStringAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
        }

        private async Task ClearProductCacheAsync()
        {
            // Implementation depends on cache provider
            await Task.CompletedTask;
        }

        public Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto productDto, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ProductDto>> GetProductByIdAsync(long productId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ProductDto>> GetProductBySKUAsync(string sku)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> DeleteProductAsync(long productId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<InventoryItemDto>> GetInventoryItemAsync(long productId, long warehouseId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<InventoryItemDto>>> GetInventoryByProductAsync(long productId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<InventoryItemDto>>> GetInventoryByWarehouseAsync(long warehouseId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> TransferInventoryAsync(InventoryTransferDto transferDto, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> AdjustInventoryAsync(long productId, long warehouseId, int adjustmentQuantity, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<LowStockAlertDto>>> GetLowStockItemsAsync(long? warehouseId = null)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<InventoryItemDto>>> GetOutOfStockItemsAsync(long? warehouseId = null)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(long? productId, long? warehouseId, DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ReleaseReservationAsync(long orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<List<InventoryReservationDto>>> GetActiveReservationsAsync(long productId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ProcessExpiredReservationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
