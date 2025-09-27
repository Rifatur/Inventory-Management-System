using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    /// <summary>
    /// DTO for inventory transfer between warehouses
    /// </summary>
    public class InventoryTransferDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Source warehouse is required")]
        public long FromWarehouseId { get; set; }

        [Required(ErrorMessage = "Destination warehouse is required")]
        public long ToWarehouseId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
