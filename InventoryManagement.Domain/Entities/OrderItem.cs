using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Entities
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderItemId { get; set; }

        /// <summary>
        /// Foreign key to Order
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Foreign key to Product
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Product SKU at time of order
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        /// <summary>
        /// Product name at time of order
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at time of order
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount amount applied
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tax amount for this item
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total price for this line item
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Fulfillment status (Pending, Picked, Packed, Shipped)
        /// </summary>
        [StringLength(50)]
        public string FulfillmentStatus { get; set; }

        public long? FulfilledFromWarehouseId { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual Warehouse FulfilledFromWarehouse { get; set; }
    }
}
