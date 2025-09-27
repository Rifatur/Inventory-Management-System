using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit price must be between 0.01 and 999999.99")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Unit cost is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit cost must be between 0.01 and 999999.99")]
        public decimal UnitCost { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be non-negative")]
        public int ReorderLevel { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Reorder quantity must be at least 1")]
        public int ReorderQuantity { get; set; }

        [Range(0, 9999.999, ErrorMessage = "Weight must be between 0 and 9999.999 kg")]
        public decimal Weight { get; set; }

        public string Barcode { get; set; }
    }
}
