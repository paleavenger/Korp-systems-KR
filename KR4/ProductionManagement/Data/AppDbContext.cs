using KR5.Models;
using Microsoft.EntityFrameworkCore;

namespace KR5.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductionLine> ProductionLines { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProductMaterial> ProductMaterials { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMaterial>()
            .HasKey(pm => new { pm.ProductId, pm.MaterialId });

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Product)
            .WithMany(p => p.ProductMaterials)
            .HasForeignKey(pm => pm.ProductId);

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Material)
            .WithMany(m => m.ProductMaterials)
            .HasForeignKey(pm => pm.MaterialId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(w => w.Product)
            .WithMany(p => p.WorkOrders)
            .HasForeignKey(w => w.ProductId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(w => w.ProductionLine)
            .WithMany(l => l.WorkOrders)
            .HasForeignKey(w => w.ProductionLineId)
            .IsRequired(false);

        // Seed data
        modelBuilder.Entity<Material>().HasData(
            new Material { Id = 1, Name = "Сталь", Quantity = 500, UnitOfMeasure = "кг", MinimalStock = 100 },
            new Material { Id = 2, Name = "Резина", Quantity = 200, UnitOfMeasure = "кг", MinimalStock = 50 },
            new Material { Id = 3, Name = "Пластик", Quantity = 80, UnitOfMeasure = "кг", MinimalStock = 100 },
            new Material { Id = 4, Name = "Алюминий", Quantity = 350, UnitOfMeasure = "кг", MinimalStock = 75 }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Шестерня А", Description = "Стальная шестерня для редуктора", Specifications = "{\"диаметр\":\"120мм\",\"зубья\":24}", Category = "Механика", MinimalStock = 50, ProductionTimePerUnit = 30 },
            new Product { Id = 2, Name = "Корпус Б", Description = "Пластиковый корпус для электроники", Specifications = "{\"размер\":\"200x150x80мм\",\"цвет\":\"черный\"}", Category = "Корпуса", MinimalStock = 30, ProductionTimePerUnit = 20 },
            new Product { Id = 3, Name = "Прокладка В", Description = "Резиновая прокладка уплотнительная", Specifications = "{\"диаметр\":\"50мм\",\"толщина\":\"5мм\"}", Category = "Уплотнители", MinimalStock = 200, ProductionTimePerUnit = 5 }
        );

        modelBuilder.Entity<ProductionLine>().HasData(
            new ProductionLine { Id = 1, Name = "Линия №1", Status = "Active", EfficiencyFactor = 1.0f },
            new ProductionLine { Id = 2, Name = "Линия №2", Status = "Stopped", EfficiencyFactor = 1.2f },
            new ProductionLine { Id = 3, Name = "Линия №3", Status = "Active", EfficiencyFactor = 0.8f }
        );

        modelBuilder.Entity<ProductMaterial>().HasData(
            new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 2.5m },
            new ProductMaterial { ProductId = 1, MaterialId = 4, QuantityNeeded = 0.5m },
            new ProductMaterial { ProductId = 2, MaterialId = 3, QuantityNeeded = 0.8m },
            new ProductMaterial { ProductId = 3, MaterialId = 2, QuantityNeeded = 0.2m }
        );

        modelBuilder.Entity<WorkOrder>().HasData(
            new WorkOrder
            {
                Id = 1, ProductId = 1, ProductionLineId = 1, Quantity = 10,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EstimatedEndDate = DateTime.UtcNow.AddHours(4),
                Status = "InProgress", Progress = 45, MaterialsConsumed = true
            },
            new WorkOrder
            {
                Id = 2, ProductId = 2, ProductionLineId = null, Quantity = 20,
                StartDate = DateTime.UtcNow,
                EstimatedEndDate = DateTime.UtcNow.AddHours(7),
                Status = "Pending", Progress = 0, MaterialsConsumed = false
            }
        );
    }
}
