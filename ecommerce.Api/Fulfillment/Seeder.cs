using ecommerce.Data;
using ecommerce.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Api.Fulfillment;

// In "production" our orders would come from users. These APIs run locally
// so we could either - create a post for a single order and run a shell script or something
// or we could create seeding endpoint from here to generate some orders for us
public interface ISeeder
{
    IReadOnlyList<int> SeedOrders(int n, bool expedited);
    IReadOnlyList<int> ResetAndCreateOrders(int n);
}

public class Seeder : ISeeder
{
    private static readonly string[] Skus =
    {
        "TS-001",
        "TS-002",
        "SH-001",
        "SH-002",
        "SW-001",
        "SW-002",
        "HD-001",
        "HD-002",
        "JN-001",
        "ST-001"
    };

    private readonly IDbContextFactory<EcommerceDbContext> _factory;

    public Seeder(IDbContextFactory<EcommerceDbContext> factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<int> SeedOrders(int n, bool expedited)
    {
        using var db = _factory.CreateDbContext();

        var productsBySku = db.Products.ToDictionary(p => p.Sku, p => p);

        var ids = new List<int>(n);

        for (int i = 0; i < n; i++)
        {
            string sku = Skus[i % Skus.Length];
            Product product = productsBySku[sku];

            var order = new Order
            {
                CustomerId = Random.Shared.Next(1, 5),
                Priority = expedited ? Priority.Expedited : Priority.Normal,
                Status = OrderStatus.Pending,
                Items =
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.Price
                    }
                }
            };

            db.Orders.Add(order);
            db.SaveChanges();

            ids.Add(order.Id);
        }

        return ids;
    }

    public IReadOnlyList<int> ResetAndCreateOrders(int n)
    {
        using var db = _factory.CreateDbContext();

        // Reset inventory to the same values used in EcommerceDbContext seed data.
        var startingStock = new Dictionary<int, int>
        {
            { 1, 20 },
            { 2, 18 },
            { 3, 15 },
            { 4, 10 },
            { 5, 12 },
            { 6, 9 },
            { 7, 12 },
            { 8, 11 },
            { 9, 8 },
            { 10, 14 }
        };

        foreach (ProductInventory inv in db.Inventory)
        {
            if (startingStock.TryGetValue(inv.ProductId, out int stock))
            {
                inv.CurrentStock = stock;
            }
        }

        db.SaveChanges();

        var productsBySku = db.Products.ToDictionary(p => p.Sku, p => p);

        var ids = new List<int>(n);

        for (int i = 0; i < n; i++)
        {
            string sku = Skus[i % Skus.Length];
            Product product = productsBySku[sku];

            var order = new Order
            {
                CustomerId = Random.Shared.Next(1,5),
                Priority = i % 3 == 0 ? Priority.Expedited : Priority.Normal,
                Status = OrderStatus.Pending,
                Items =
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.Price
                    }
                }
            };

            db.Orders.Add(order);
            db.SaveChanges();

            ids.Add(order.Id);
        }

        return ids;
    }
}