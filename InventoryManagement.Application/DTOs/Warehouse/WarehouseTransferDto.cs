using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class WarehouseTransferDto
    {
        [Required(ErrorMessage = "Source warehouse is required")]
        public long FromWarehouseId { get; set; }

        [Required(ErrorMessage = "Destination warehouse is required")]
        public long ToWarehouseId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; }

        public List<long> ProductIds { get; set; } // If null, transfer all products

        public bool TransferReservedStock { get; set; } = false;
    }
}
