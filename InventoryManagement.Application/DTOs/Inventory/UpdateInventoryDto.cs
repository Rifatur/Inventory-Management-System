using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    /// <summary>
    /// DTO for updating inventory levels
    /// </summary>
    public class UpdateInventoryDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Warehouse ID is required")]
        public long WarehouseId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement type is required")]
        public string MovementType { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal? UnitCost { get; set; }
    }
}
