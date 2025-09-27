using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Stock
{
    public class StockMovementDto
    {
        public long MovementId { get; set; }
        public long ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public long? FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; }
        public long? ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; }
        public string MovementType { get; set; }
        public int Quantity { get; set; }
        public string ReferenceNumber { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }
}
