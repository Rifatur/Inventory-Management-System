using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for product response
    /// </summary>
    public class ProductDto
    {
        public long ProductId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public decimal Weight { get; set; }
        public string Barcode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// Total quantity across all warehouses
        /// </summary>
        public int TotalQuantityOnHand { get; set; }
        /// <summary>
        /// Total available quantity across all warehouses
        /// </summary>
        public int TotalQuantityAvailable { get; set; }
    }
}
