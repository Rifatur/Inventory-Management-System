using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    /// <summary>
    /// DTO for inventory item details
    /// </summary>
    public class InventoryItemDto
    {
        public long InventoryId { get; set; }
        public long ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable { get; set; }
        public string BinLocation { get; set; } = string.Empty;
        public DateTime? LastStockTakeDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
