using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Stock
{
    public class LowStockAlertDto
    {
        public long ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public List<WarehouseStockDto> WarehouseStock { get; set; }
    }
}
