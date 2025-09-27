using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Stock
{
    public class WarehouseStockDto
    {
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
