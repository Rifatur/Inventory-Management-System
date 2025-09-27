using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Order
{

    public class CreateOrderItemDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public long ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Discount must be between 0 and 999999.99")]
        public decimal DiscountAmount { get; set; }
    }
}
