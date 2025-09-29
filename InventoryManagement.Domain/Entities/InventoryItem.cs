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
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int QuantityAvailable { get; set; }
        [StringLength(50)]
        public string BinLocation { get; set; }
        public DateTime? LastStockTakeDate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual ICollection<InventoryReservation> Reservations { get; set; }
    }

}
