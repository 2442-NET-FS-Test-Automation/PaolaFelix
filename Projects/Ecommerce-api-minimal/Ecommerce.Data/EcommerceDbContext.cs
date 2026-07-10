using Microsoft.EntityFrameworkCore;
using ecommerce.Data.Entities;
using System.Dynamic;

namespace ecommerce.Data;

// DbContext is EF Core's bridge between our app and the database.
// It tells EF which classes should become tables and how they relate to each other.
public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    // These DbSets become database tables
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductInventory> Inventory => Set<ProductInventory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTracking> OrderTracking => Set<OrderTracking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Product rules and relationships
        b.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Sku).IsUnique();

            e.Property(p => p.Price)
                .HasColumnType("decimal(10,2)");

            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            e.HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<ProductInventory>(i => i.ProductId);
        });

        // RowVersion helps prevent two updates from overwriting inventory at the same time
        b.Entity<ProductInventory>()
            .Property(i => i.RowVersion)
            .IsRowVersion();

        // Customer emails should be unique
        b.Entity<Customer>(e =>
        {
            e.Property(c => c.Email)
                .HasMaxLength(256);

            e.HasIndex(c => c.Email)
                .IsUnique();
        });

        // One customer can place many orders
        b.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId);

        // One order contains many order items
        b.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId);

        // Each order item points to the product being purchased
        b.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId);

        // One order can have many tracking updates
        b.Entity<OrderTracking>()
            .HasOne(t => t.Order)
            .WithMany(o => o.TrackingHistory)
            .HasForeignKey(t => t.OrderId);

        // Seed categories first because products need CategoryId
        b.Entity<Category>().HasData(
        new Category { Id = 1, Name = "T-Shirts" },
        new Category { Id = 2, Name = "Shirts" },
        new Category { Id = 3, Name = "Sweaters" },
        new Category { Id = 4, Name = "Hoodies" },
        new Category { Id = 5, Name = "Jeans" },
        new Category { Id = 6, Name = "Shorts" }
        );

        // Seed clothing products
        b.Entity<Product>().HasData(
        new Product { Id = 1, CategoryId = 1, Sku = "TS-001", Name = "Graphic Tee", Description = "100% cotton graphic t-shirt", Size = ProductSize.M, Color = "Black", Price = 400m },
        new Product { Id = 2, CategoryId = 1, Sku = "TS-002", Name = "Basic Tee", Description = "Plain everyday t-shirt", Size = ProductSize.S, Color = "White", Price = 300m },
        new Product { Id = 3, CategoryId = 2, Sku = "SH-001", Name = "Button-Up Shirt", Description = "Casual button-up shirt", Size = ProductSize.M, Color = "Blue", Price = 550m },
        new Product { Id = 4, CategoryId = 2, Sku = "SH-002", Name = "Linen Shirt", Description = "Lightweight linen shirt", Size = ProductSize.L, Color = "Beige", Price = 650m },
        new Product { Id = 5, CategoryId = 3, Sku = "SW-001", Name = "Crewneck Sweater", Description = "Soft knit crewneck sweater", Size = ProductSize.M, Color = "Cream", Price = 750m },
        new Product { Id = 6, CategoryId = 3, Sku = "SW-002", Name = "Turtleneck Sweater", Description = "Warm turtleneck sweater", Size = ProductSize.L, Color = "Brown", Price = 850m },
        new Product { Id = 7, CategoryId = 4, Sku = "HD-001", Name = "Oversized Hoodie", Description = "Cotton blend oversized hoodie", Size = ProductSize.L, Color = "Gray", Price = 800m },
        new Product { Id = 8, CategoryId = 4, Sku = "HD-002", Name = "Zip-Up Hoodie", Description = "Comfortable zip-up hoodie", Size = ProductSize.M, Color = "Navy", Price = 850m },
        new Product { Id = 9, CategoryId = 5, Sku = "JN-001", Name = "Dark Denim Jeans", Description = "Classic dark denim jeans", Size = ProductSize.XL, Color = "Blue", Price = 600m },
        new Product { Id = 10, CategoryId = 6, Sku = "ST-001", Name = "Casual Shorts", Description = "Lightweight casual shorts", Size = ProductSize.M, Color = "Khaki", Price = 450m }
        );

        b.Entity<ProductInventory>().HasData(
            new ProductInventory { Id = 1, ProductId = 1, CurrentStock = 20 },
            new ProductInventory { Id = 2, ProductId = 2, CurrentStock = 18 },
            new ProductInventory { Id = 3, ProductId = 3, CurrentStock = 15 },
            new ProductInventory { Id = 4, ProductId = 4, CurrentStock = 10 },
            new ProductInventory { Id = 5, ProductId = 5, CurrentStock = 12 },
            new ProductInventory { Id = 6, ProductId = 6, CurrentStock = 9 },
            new ProductInventory { Id = 7, ProductId = 7, CurrentStock = 12 },
            new ProductInventory { Id = 8, ProductId = 8, CurrentStock = 11 },
            new ProductInventory { Id = 9, ProductId = 9, CurrentStock = 8 },
            new ProductInventory { Id = 10, ProductId = 10, CurrentStock = 14 }
        );

        // HasData runs inside the migration BEFORE SQL Server can hand out identity keys
        // Which is why we give explicict PKs when seeding
        b.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Lucia Rodriguez", Email = "lucia@example.com" },
            new Customer { Id = 2, Name = "Juan Perez", Email = "juan@example.com" },
            new Customer { Id = 3, Name = "Paco Sanchez", Email = "paco@example.com" },
            new Customer { Id = 4, Name = "Ana Lopez", Email = "ana@example.com" }
        );
    }
}

