using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryManagement.Infrastructure.Data
{
    /// <summary>
    /// Main database context for the Inventory Management System
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<InventoryReservation> InventoryReservations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Apply global query filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);

            // Configure decimal precision globally
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // Seed initial data
            SeedData(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update audit fields
            UpdateAuditableEntities();

            // Save audit logs
            var auditEntries = OnBeforeSaveChanges();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Save audit entries after successful save
            if (auditEntries != null && auditEntries.Any())
            {
                foreach (var auditEntry in auditEntries)
                {
                    AuditLogs.Add(auditEntry.ToAuditLog());
                }
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = GetCurrentUser();
                }
                else
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = GetCurrentUser();
                }
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = GetCurrentUser()
                };

                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.AuditType = AuditType.Create;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.AuditType = AuditType.Delete;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                auditEntry.AuditType = AuditType.Update;
                            }
                            break;
                    }
                }
            }

            return auditEntries;
        }

        private string GetCurrentUser()
        {
            // This should be implemented to get the current user from the context
            // For now, returning a default value
            return "System";
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse
                {
                    WarehouseId = 1,
                    WarehouseCode = "WH-MAIN",
                    Name = "Main Warehouse",
                    WarehouseType = "Main",
                    Address = "123 Main Street",
                    City = "New York",
                    State = "NY",
                    Country = "US",
                    PostalCode = "10001",
                    Phone = "+1-212-555-0100",
                    ManagerEmail = "manager.main@company.com",
                    MaxCapacity = 10000,
                    CurrentUtilization = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Warehouse
                {
                    WarehouseId = 2,
                    WarehouseCode = "WH-WEST",
                    Name = "West Coast Distribution Center",
                    WarehouseType = "Regional",
                    Address = "456 Pacific Avenue",
                    City = "Los Angeles",
                    State = "CA",
                    Country = "US",
                    PostalCode = "90001",
                    Phone = "+1-213-555-0200",
                    ManagerEmail = "manager.west@company.com",
                    MaxCapacity = 8000,
                    CurrentUtilization = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Product Categories (as configuration data)
            // Note: Actual products should be added through the application
        }

        // Transaction management methods
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _currentTransaction?.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public bool HasActiveTransaction => _currentTransaction != null;
    }

    // Audit helper classes
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public AuditType AuditType { get; set; }
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            var audit = new AuditLog
            {
                UserId = 002895,
                EntityName = TableName,
                Action = AuditType.ToString(),
                Timestamp = DateTime.UtcNow,
                EntityId = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues)
            };

            return audit;
        }
    }

    public enum AuditType
    {
        Create,
        Update,
        Delete
    }

    // Interface for auditable entities
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedBy { get; set; }
    }

    // Interface for soft-deletable entities
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string DeletedBy { get; set; }
    }
}
