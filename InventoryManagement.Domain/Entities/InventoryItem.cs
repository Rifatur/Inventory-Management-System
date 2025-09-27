using Microsoft.EntityFrameworkCore;
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
    /// Represents inventory levels at specific warehouse locations
    /// </summary>
    [Table("InventoryItems")]
    [Index(nameof(ProductId), nameof(WarehouseId), IsUnique = true)]
    public class InventoryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InventoryId { get; set; }

        /// <summary>
        /// Foreign key to Product
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Foreign key to Warehouse
        /// </summary>
        public long WarehouseId { get; set; }

        /// <summary>
        /// Current quantity physically in stock
        /// </summary>
        public int QuantityOnHand { get; set; }

        /// <summary>
        /// Quantity reserved for pending orders
        /// </summary>
        public int QuantityReserved { get; set; }

        /// <summary>
        /// Quantity available for sale (OnHand - Reserved)
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Specific location within the warehouse (e.g., "A-12-3")
        /// </summary>
        [StringLength(50)]
        public string BinLocation { get; set; }

        /// <summary>
        /// Last time inventory was physically counted
        /// </summary>
        public DateTime? LastStockTakeDate { get; set; }

        /// <summary>
        /// Timestamp of last update
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual ICollection<InventoryReservation> Reservations { get; set; }
    }

}
