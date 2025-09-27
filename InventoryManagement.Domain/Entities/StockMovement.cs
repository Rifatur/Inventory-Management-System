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
    /// Tracks all inventory movements
    /// </summary>
    [Table("StockMovements")]
    public class StockMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MovementId { get; set; }
        public long ProductId { get; set; }
        public long? FromWarehouseId { get; set; }
        public long? ToWarehouseId { get; set; }
        /// Movement type (Receipt, Shipment, Transfer, Adjustment, Return)
        [Required]
        [StringLength(50)]
        public string MovementType { get; set; }

        /// <summary>
        /// Quantity moved (positive for receipts, negative for shipments)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Reference document number (PO, Order, Transfer)
        /// </summary>
        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Warehouse FromWarehouse { get; set; }
        public virtual Warehouse ToWarehouse { get; set; }
    }


}
