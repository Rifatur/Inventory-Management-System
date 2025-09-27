using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    public class InventorySummaryDto
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public List<CategorySummaryDto> CategoryBreakdown { get; set; }
        public List<WarehouseSummaryDto> WarehouseBreakdown { get; set; }
    }
}
