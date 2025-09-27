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
    /// Audit log for tracking all system changes
    /// </summary>
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditId { get; set; }
        /// Entity type that was modified
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; }
        /// Primary key of the modified entity
        [Required]
        [StringLength(50)]
        public string EntityId { get; set; }
        /// Type of action (Create, Update, Delete)
        [Required]
        [StringLength(50)]
        public string Action { get; set; }
        [Required]
        [StringLength(100)]
        public long UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        /// JSON representation of old values
        public string OldValues { get; set; }
        /// JSON representation of new values
        public string NewValues { get; set; }
        [StringLength(50)]
        public string IpAddress { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
    }
}
