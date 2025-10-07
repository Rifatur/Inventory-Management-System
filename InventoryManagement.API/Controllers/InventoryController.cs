using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Inventory;
using InventoryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagement.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets inventory item by product and warehouse
        /// </summary>

        [HttpGet("{productId}/{warehouseId}")]

        public async Task<IActionResult> GetInventoryItem(int productId, int warehouseId)
        {
            try
            {
                var result = await _inventoryService.GetInventoryItemAsync(productId, warehouseId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory for Product: {ProductId}, Warehouse: {WarehouseId}",
                    productId, warehouseId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving inventory",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Gets all inventory items for a product across all warehouses
        /// </summary>

        [HttpGet("product/{productId}")]

        public async Task<IActionResult> GetInventoryByProduct(int productId)
        {
            try
            {
                var result = await _inventoryService.GetInventoryByProductAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory for product {ProductId}", productId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving inventory",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Gets all inventory items in a warehouse
        /// </summary>

        [HttpGet("warehouse/{warehouseId}")]

        public async Task<IActionResult> GetInventoryByWarehouse(int warehouseId)
        {
            try
            {
                var result = await _inventoryService.GetInventoryByWarehouseAsync(warehouseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving inventory",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Updates inventory levels
        /// </summary>

        [HttpPost("update")]
        [Authorize(Roles = "Admin,WarehouseManager,InventoryClerk")]

        public async Task<IActionResult> UpdateInventory([FromBody] UpdateInventoryDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                var result = await _inventoryService.UpdateInventoryAsync(updateDto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating inventory",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Transfers inventory between warehouses
        /// </summary>

        [HttpPost("transfer")]
        [Authorize(Roles = "Admin,WarehouseManager")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferInventory([FromBody] InventoryTransferDto transferDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (transferDto.FromWarehouseId == transferDto.ToWarehouseId)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Source and destination warehouses cannot be the same"
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                var result = await _inventoryService.TransferInventoryAsync(transferDto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring inventory");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while transferring inventory",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }
    }
}
