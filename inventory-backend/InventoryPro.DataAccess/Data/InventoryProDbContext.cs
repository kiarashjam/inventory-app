using InventoryPro.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.DataAccess.Data;

public class InventoryProDbContext : IdentityDbContext<AppUser>
{
    public InventoryProDbContext(DbContextOptions<InventoryProDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemStockMapping> MenuItemStockMappings => Set<MenuItemStockMapping>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockCategory> StockCategories => Set<StockCategory>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<StockItemStorageLocation> StockItemStorageLocations => Set<StockItemStorageLocation>();
    public DbSet<AlternateCountUnit> AlternateCountUnits => Set<AlternateCountUnit>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierStockItem> SupplierStockItems => Set<SupplierStockItem>();
    public DbSet<SupplierCertification> SupplierCertifications => Set<SupplierCertification>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceiving> GoodsReceivings => Set<GoodsReceiving>();
    public DbSet<GoodsReceivingItem> GoodsReceivingItems => Set<GoodsReceivingItem>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountItem> StockCountItems => Set<StockCountItem>();
    public DbSet<WasteRecord> WasteRecords => Set<WasteRecord>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();
    public DbSet<PosConnection> PosConnections => Set<PosConnection>();
    public DbSet<ExternalProductMapping> ExternalProductMappings => Set<ExternalProductMapping>();
    public DbSet<SaleRecord> SaleRecords => Set<SaleRecord>();
    public DbSet<SaleRecordItem> SaleRecordItems => Set<SaleRecordItem>();
    public DbSet<AllergenTag> AllergenTags => Set<AllergenTag>();
    public DbSet<StockItemAllergen> StockItemAllergens => Set<StockItemAllergen>();
    public DbSet<StockItemNutrition> StockItemNutritions => Set<StockItemNutrition>();
    public DbSet<HaccpChecklistTemplate> HaccpChecklistTemplates => Set<HaccpChecklistTemplate>();
    public DbSet<HaccpChecklistItem> HaccpChecklistItems => Set<HaccpChecklistItem>();
    public DbSet<HaccpChecklistLog> HaccpChecklistLogs => Set<HaccpChecklistLog>();
    public DbSet<HaccpChecklistLogItem> HaccpChecklistLogItems => Set<HaccpChecklistLogItem>();
    public DbSet<PrepList> PrepLists => Set<PrepList>();
    public DbSet<PrepListItem> PrepListItems => Set<PrepListItem>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs => Set<WebhookDeliveryLog>();
    public DbSet<InventorySnapshot> InventorySnapshots => Set<InventorySnapshot>();
    public DbSet<InventorySnapshotItem> InventorySnapshotItems => Set<InventorySnapshotItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Organization
        builder.Entity<Organization>(e =>
        {
            e.HasIndex(o => o.Slug).IsUnique();
            e.Property(o => o.Name).HasMaxLength(200).IsRequired();
            e.Property(o => o.Slug).HasMaxLength(100);
            e.Property(o => o.Currency).HasMaxLength(10).HasDefaultValue("CHF");
        });

        // OrganizationUser
        builder.Entity<OrganizationUser>(e =>
        {
            e.HasOne(ou => ou.Organization).WithMany().HasForeignKey(ou => ou.OrganizationId);
            e.HasOne(ou => ou.AppUser).WithMany().HasForeignKey(ou => ou.UserId);
            e.HasIndex(ou => new { ou.OrganizationId, ou.UserId }).IsUnique();
        });

        // MenuItem
        builder.Entity<MenuItem>(e =>
        {
            e.HasOne(m => m.Organization).WithMany().HasForeignKey(m => m.OrganizationId);
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.Property(m => m.SellingPrice).HasColumnType("decimal(18,2)");
        });

        // StockItem
        builder.Entity<StockItem>(e =>
        {
            e.HasOne(s => s.Organization).WithMany().HasForeignKey(s => s.OrganizationId);
            e.HasOne(s => s.Category).WithMany().HasForeignKey(s => s.CategoryId);
            e.HasOne(s => s.PrimarySupplier).WithMany().HasForeignKey(s => s.PrimarySupplierId);
            e.HasOne(s => s.PrimaryStorageLocation).WithMany().HasForeignKey(s => s.PrimaryStorageLocationId);
            e.HasIndex(s => new { s.OrganizationId, s.SKU }).IsUnique().HasFilter("[SKU] IS NOT NULL");
            e.Property(s => s.Name).HasMaxLength(200).IsRequired();
            e.Property(s => s.SKU).HasMaxLength(50);
            e.Property(s => s.CurrentQuantity).HasColumnType("decimal(18,4)");
            e.Property(s => s.CostPrice).HasColumnType("decimal(18,4)");
            e.Property(s => s.AverageCostPrice).HasColumnType("decimal(18,4)");
            e.Property(s => s.RowVersion).IsRowVersion();
        });

        // StockCategory
        builder.Entity<StockCategory>(e =>
        {
            e.HasOne(c => c.Organization).WithMany().HasForeignKey(c => c.OrganizationId);
            e.HasOne(c => c.ParentCategory).WithMany(c => c.SubCategories).HasForeignKey(c => c.ParentCategoryId);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // StorageLocation
        builder.Entity<StorageLocation>(e =>
        {
            e.HasOne(l => l.Organization).WithMany().HasForeignKey(l => l.OrganizationId);
            e.Property(l => l.Name).HasMaxLength(100).IsRequired();
        });

        // StockItemStorageLocation
        builder.Entity<StockItemStorageLocation>(e =>
        {
            e.HasOne(sl => sl.StockItem).WithMany().HasForeignKey(sl => sl.StockItemId);
            e.HasOne(sl => sl.StorageLocation).WithMany().HasForeignKey(sl => sl.StorageLocationId);
            e.HasIndex(sl => new { sl.StockItemId, sl.StorageLocationId }).IsUnique();
            e.Property(sl => sl.Quantity).HasColumnType("decimal(18,4)");
        });

        // MenuItemStockMapping
        builder.Entity<MenuItemStockMapping>(e =>
        {
            e.HasOne(m => m.MenuItem).WithMany().HasForeignKey(m => m.MenuItemId);
            e.HasOne(m => m.StockItem).WithMany().HasForeignKey(m => m.StockItemId);
            e.HasIndex(m => new { m.MenuItemId, m.StockItemId }).IsUnique();
            e.Property(m => m.QuantityRequired).HasColumnType("decimal(18,4)");
        });

        // Supplier
        builder.Entity<Supplier>(e =>
        {
            e.HasOne(s => s.Organization).WithMany().HasForeignKey(s => s.OrganizationId);
            e.Property(s => s.Name).HasMaxLength(200).IsRequired();
        });

        // SupplierStockItem
        builder.Entity<SupplierStockItem>(e =>
        {
            e.HasOne(ss => ss.Supplier).WithMany().HasForeignKey(ss => ss.SupplierId);
            e.HasOne(ss => ss.StockItem).WithMany().HasForeignKey(ss => ss.StockItemId);
            e.HasIndex(ss => new { ss.SupplierId, ss.StockItemId }).IsUnique();
            e.Property(ss => ss.UnitPrice).HasColumnType("decimal(18,4)");
        });

        // StockMovement
        builder.Entity<StockMovement>(e =>
        {
            e.HasOne(m => m.StockItem).WithMany().HasForeignKey(m => m.StockItemId);
            e.Property(m => m.Quantity).HasColumnType("decimal(18,4)");
            e.Property(m => m.CostPerUnit).HasColumnType("decimal(18,4)");
            e.HasIndex(m => new { m.OrganizationId, m.CreatedAt });
        });

        // PurchaseOrder
        builder.Entity<PurchaseOrder>(e =>
        {
            e.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId);
            e.HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId);
            e.HasMany(p => p.Items).WithOne(i => i.PurchaseOrder).HasForeignKey(i => i.PurchaseOrderId);
            e.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.OrderNumber).HasMaxLength(50);
        });

        // PurchaseOrderItem
        builder.Entity<PurchaseOrderItem>(e =>
        {
            e.HasOne(i => i.StockItem).WithMany().HasForeignKey(i => i.StockItemId);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(18,4)");
            e.Property(i => i.TotalPrice).HasColumnType("decimal(18,2)");
        });

        // GoodsReceiving
        builder.Entity<GoodsReceiving>(e =>
        {
            e.HasOne(g => g.Organization).WithMany().HasForeignKey(g => g.OrganizationId);
            e.HasOne(g => g.PurchaseOrder).WithMany().HasForeignKey(g => g.PurchaseOrderId);
            e.HasMany(g => g.Items).WithOne(i => i.GoodsReceiving).HasForeignKey(i => i.GoodsReceivingId);
        });

        // StockCount
        builder.Entity<StockCount>(e =>
        {
            e.HasOne(c => c.Organization).WithMany().HasForeignKey(c => c.OrganizationId);
            e.HasMany(c => c.Items).WithOne(i => i.StockCount).HasForeignKey(i => i.StockCountId);
            e.HasIndex(c => new { c.OrganizationId, c.CountDate });
        });

        // WasteRecord
        builder.Entity<WasteRecord>(e =>
        {
            e.HasOne(w => w.Organization).WithMany().HasForeignKey(w => w.OrganizationId);
            e.HasOne(w => w.StockItem).WithMany().HasForeignKey(w => w.StockItemId);
            e.Property(w => w.CostPerUnit).HasColumnType("decimal(18,4)");
            e.Property(w => w.TotalCost).HasColumnType("decimal(18,2)");
        });

        // StockAlert
        builder.Entity<StockAlert>(e =>
        {
            e.HasOne(a => a.Organization).WithMany().HasForeignKey(a => a.OrganizationId);
            e.HasOne(a => a.StockItem).WithMany().HasForeignKey(a => a.StockItemId);
            e.HasIndex(a => new { a.OrganizationId, a.IsRead });
        });

        // PosConnection
        builder.Entity<PosConnection>(e =>
        {
            e.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId);
            e.Property(p => p.ApiKeyHash).HasMaxLength(500);
        });

        // ExternalProductMapping
        builder.Entity<ExternalProductMapping>(e =>
        {
            e.HasOne(m => m.PosConnection).WithMany().HasForeignKey(m => m.PosConnectionId);
            e.HasOne(m => m.MenuItem).WithMany().HasForeignKey(m => m.MenuItemId);
            e.HasIndex(m => new { m.PosConnectionId, m.ExternalProductId }).IsUnique();
        });

        // AllergenTag seed data (14 EU allergens)
        builder.Entity<AllergenTag>().HasData(
            new AllergenTag { Id = 1, Name = "Gluten", Icon = "gluten", DisplayOrder = 1 },
            new AllergenTag { Id = 2, Name = "Crustaceans", Icon = "crustaceans", DisplayOrder = 2 },
            new AllergenTag { Id = 3, Name = "Eggs", Icon = "eggs", DisplayOrder = 3 },
            new AllergenTag { Id = 4, Name = "Fish", Icon = "fish", DisplayOrder = 4 },
            new AllergenTag { Id = 5, Name = "Peanuts", Icon = "peanuts", DisplayOrder = 5 },
            new AllergenTag { Id = 6, Name = "Soybeans", Icon = "soybeans", DisplayOrder = 6 },
            new AllergenTag { Id = 7, Name = "Milk", Icon = "milk", DisplayOrder = 7 },
            new AllergenTag { Id = 8, Name = "Nuts", Icon = "nuts", DisplayOrder = 8 },
            new AllergenTag { Id = 9, Name = "Celery", Icon = "celery", DisplayOrder = 9 },
            new AllergenTag { Id = 10, Name = "Mustard", Icon = "mustard", DisplayOrder = 10 },
            new AllergenTag { Id = 11, Name = "Sesame", Icon = "sesame", DisplayOrder = 11 },
            new AllergenTag { Id = 12, Name = "Sulphites", Icon = "sulphites", DisplayOrder = 12 },
            new AllergenTag { Id = 13, Name = "Lupin", Icon = "lupin", DisplayOrder = 13 },
            new AllergenTag { Id = 14, Name = "Molluscs", Icon = "molluscs", DisplayOrder = 14 }
        );

        // StockItemAllergen
        builder.Entity<StockItemAllergen>(e =>
        {
            e.HasOne(sa => sa.StockItem).WithMany(s => s.AllergenTags).HasForeignKey(sa => sa.StockItemId);
            e.HasOne(sa => sa.AllergenTag).WithMany().HasForeignKey(sa => sa.AllergenTagId);
        });

        // StockItemNutrition (one-to-one)
        builder.Entity<StockItemNutrition>(e =>
        {
            e.HasOne(n => n.StockItem).WithOne(s => s.NutritionalInfo).HasForeignKey<StockItemNutrition>(n => n.StockItemId);
        });

        // HACCP
        builder.Entity<HaccpChecklistTemplate>(e =>
        {
            e.HasOne(t => t.Organization).WithMany().HasForeignKey(t => t.OrganizationId);
            e.HasMany(t => t.Items).WithOne(i => i.Template).HasForeignKey(i => i.TemplateId);
        });

        builder.Entity<HaccpChecklistLog>(e =>
        {
            e.HasOne(l => l.Organization).WithMany().HasForeignKey(l => l.OrganizationId);
            e.HasOne(l => l.Template).WithMany().HasForeignKey(l => l.TemplateId);
            e.HasMany(l => l.Items).WithOne(i => i.Log).HasForeignKey(i => i.LogId);
        });

        // PrepList
        builder.Entity<PrepList>(e =>
        {
            e.HasOne(p => p.Organization).WithMany().HasForeignKey(p => p.OrganizationId);
            e.HasMany(p => p.Items).WithOne(i => i.PrepList).HasForeignKey(i => i.PrepListId);
        });

        // WebhookSubscription
        builder.Entity<WebhookSubscription>(e =>
        {
            e.HasOne(w => w.PosConnection).WithMany().HasForeignKey(w => w.PosConnectionId);
        });

        // InventorySnapshot
        builder.Entity<InventorySnapshot>(e =>
        {
            e.HasOne(s => s.Organization).WithMany().HasForeignKey(s => s.OrganizationId);
            e.HasMany(s => s.Items).WithOne(i => i.Snapshot).HasForeignKey(i => i.SnapshotId);
        });

        // SaleRecord
        builder.Entity<SaleRecord>(e =>
        {
            e.HasOne(s => s.Organization).WithMany().HasForeignKey(s => s.OrganizationId);
            e.HasMany(s => s.Items).WithOne(i => i.SaleRecord).HasForeignKey(i => i.SaleRecordId);
        });

        // SaleRecordItem
        builder.Entity<SaleRecordItem>(e =>
        {
            e.HasOne(i => i.SaleRecord).WithMany(s => s.Items).HasForeignKey(i => i.SaleRecordId);
            e.HasOne(i => i.MenuItem).WithMany().HasForeignKey(i => i.MenuItemId);
        });

        // UnitConversion
        builder.Entity<UnitConversion>(e =>
        {
            e.HasOne(u => u.Organization).WithMany().HasForeignKey(u => u.OrganizationId);
        });

        // SupplierCertification
        builder.Entity<SupplierCertification>(e =>
        {
            e.HasOne(sc => sc.Supplier).WithMany().HasForeignKey(sc => sc.SupplierId);
        });

        // WebhookDeliveryLog
        builder.Entity<WebhookDeliveryLog>(e =>
        {
            e.HasOne(w => w.Subscription).WithMany().HasForeignKey(w => w.SubscriptionId);
        });
    }
}
