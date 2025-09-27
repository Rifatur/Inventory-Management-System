using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Order
{
    /// <summary>
    /// DTO for creating an order
    /// </summary>
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public string CustomerId { get; set; }  

        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Shipping address is required")]
        public string ShippingAddress { get; set; } 

        public string BillingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Order items are required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<CreateOrderItemDto> OrderItems { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
    }
}
