using ecommerce.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Data;

// This repository only handles database access for products and inventory.
// Business rules should stay in services or API endpoints.
public class InventoryRepository : IInventoryRepository
{
    private readonly IDbContextFactory<EcommerceDbContext> _factory;

    public InventoryRepository(IDbContextFactory<EcommerceDbContext> factory)
    {
        _factory = factory;
    }

    // Get all inventory rows and include Product + Category
    // so the API can show useful product details.
    public async Task<IReadOnlyList<ProductInventory>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Inventory
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .ToListAsync();
    }

    // Find inventory by product SKU.
    public async Task<ProductInventory?> GetInventoryItemBySkuAsync(string sku)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Inventory
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Product.Sku == sku);
    }

    // Add a new clothing product and its inventory row together.
    public async Task<ProductInventory> AddInventoryItemAsync(string sku, string name, string description, int categoryId, ProductSize size,
        string color, decimal price, int quantity)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var newItem = new ProductInventory
        {
            Product = new Product
            {Sku = sku, Name = name, Description = description, CategoryId = categoryId, Size = size, Color = color, Price = price},
            CurrentStock = quantity
        };

        db.Inventory.Add(newItem);
        await db.SaveChangesAsync();

        return newItem;
    }

    // Remove a product by SKU.
    public async Task<bool> RemoveBySkuAsync(string sku)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var itemToRemove = await db.Inventory
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Product.Sku == sku);

        if (itemToRemove is null)
        {
            return false;
        }

        db.Products.Remove(itemToRemove.Product);
        await db.SaveChangesAsync();

        return true;
    }

    // Update stock for an existing product.
    public async Task<bool> UpdateStockBySkuAsync(string sku, int newStock)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var item = await db.Inventory
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Product.Sku == sku);

        if (item is null)
        {
            return false;
        }

        item.CurrentStock = newStock;
        await db.SaveChangesAsync();

        return true;
    }
}