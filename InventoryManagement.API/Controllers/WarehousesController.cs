using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(
            IWarehouseService warehouseService,
            ILogger<WarehousesController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all warehouses
        /// </summary>

        [HttpGet]

        public async Task<IActionResult> GetAllWarehouses([FromQuery] bool activeOnly = true)
        {
            try
            {
                _logger.LogInformation("Getting all warehouses. ActiveOnly: {ActiveOnly}", activeOnly);

                var result = await _warehouseService.GetAllWarehousesAsync(activeOnly);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving warehouses",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Gets a specific warehouse by ID
        /// </summary>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWarehouseById(long id)
        {
            try
            {
                var result = await _warehouseService.GetWarehouseByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouse {WarehouseId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the warehouse",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Gets warehouse summary with statistics
        /// </summary>

        [HttpGet("{id}/summary")]

        public async Task<IActionResult> GetWarehouseSummary(long id)
        {
            try
            {
                var result = await _warehouseService.GetWarehouseSummaryAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouse summary for {WarehouseId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving warehouse summary",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Creates a new warehouse
        /// </summary>


        [HttpPost]
        [Authorize(Roles = "Admin,WarehouseManager")]

        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto warehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _warehouseService.CreateWarehouseAsync(warehouseDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(
                    nameof(GetWarehouseById),
                    new { id = result.Data.WarehouseId },
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the warehouse",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Updates an existing warehouse
        /// </summary>

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,WarehouseManager")]

        public async Task<IActionResult> UpdateWarehouse(long id, [FromBody] UpdateWarehouseDto warehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != warehouseDto.WarehouseId)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Warehouse ID mismatch"
                    });
                }

                var result = await _warehouseService.UpdateWarehouseAsync(warehouseDto);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse {WarehouseId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the warehouse",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }

        /// <summary>
        /// Deactivates a warehouse
        /// </summary>

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeactivateWarehouse(long id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                var result = await _warehouseService.DeactivateWarehouseAsync(id, userId);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating warehouse {WarehouseId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deactivating the warehouse",
                    Errors = new List<string> { "Internal server error" }
                });
            }
        }



    }
}
