using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    public class InventorySearchDto
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public long? WarehouseId { get; set; }
        public bool? LowStockOnly { get; set; }
        public bool? OutOfStockOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "SKU";
        public bool SortDescending { get; set; } = false;
    }
}
