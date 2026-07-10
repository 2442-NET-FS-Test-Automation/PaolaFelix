using ecommerce.Data.Entities;

namespace ecommerce.Data;

public interface IInventoryRepository
{
    Task<IReadOnlyList<ProductInventory>> GetAllAsync();
    Task<ProductInventory?> GetInventoryItemBySkuAsync(string sku);
    Task<ProductInventory> AddInventoryItemAsync(string sku, string name, string description, int categoryId, ProductSize size,
        string color, decimal price, int quantity);
    Task<bool> RemoveBySkuAsync(string sku);
    Task<bool> UpdateStockBySkuAsync(string sku, int newStock);
}