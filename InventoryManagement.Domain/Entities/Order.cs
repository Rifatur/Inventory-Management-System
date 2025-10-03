using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a customer order
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }
        [StringLength(100)]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Order status (Pending, Processing, Shipped, Delivered, Cancelled)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;
        [StringLength(500)]
        public string BillingAddress { get; set; } = string.Empty;
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        [StringLength(50)]
        public string PaymentStatus { get; set; }
        [StringLength(100)]
        public string TrackingNumber { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string OrderSource { get; set; }
        public string OrderRemark { get; set; }
        public string OrderFlag { get; set; }
        public bool IsDeleted { get; set; } = false;
        public long? AssignedWarehouseId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual Warehouse AssignedWarehouse { get; set; }
    }
}
