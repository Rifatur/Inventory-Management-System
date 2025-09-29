using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a product in the inventory system
    /// </summary>
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        /// <summary>
        /// Product name for display purposes
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the product
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Product category for classification
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// Unit price of the product
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Unit cost for profit calculation
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Minimum stock level before reorder alert
        /// </summary>
        public int ReorderLevel { get; set; }

        /// <summary>
        /// Quantity to order when restocking
        /// </summary>
        public int ReorderQuantity { get; set; }

        /// <summary>
        /// Weight in kilograms for shipping calculation
        /// </summary>
        [Column(TypeName = "decimal(8,3)")]
        public decimal Weight { get; set; }

        /// <summary>
        /// Barcode for scanning operations
        /// </summary>
        [StringLength(50)]
        public string Barcode { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string UpdatedBy { get; set; }

        // Navigation properties
        //public virtual ICollection<InventoryItem> InventoryItems { get; set; }
        //public virtual ICollection<OrderItem> OrderItems { get; set; }
        //public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}
