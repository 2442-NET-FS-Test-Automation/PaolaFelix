using ecommerce.Data;
using ecommerce.Data.Entities;
using ecommerce.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Concurrent;
using ecommerce.Api.Fulfillment;

namespace ecommerce.Api.Fulfillment;

public interface IFulfillmentService
{
    Task<FulfillmentResult> FulfillOneAsync(int orderId, CancellationToken ct);
    Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct);
    int ResolveProductId(string sku);
    decimal ResolveProductPrice(string sku);
}

public enum FulfillmentResult
{
    Fulfilled,
    Backordered
}

public record BurstResult(int Fulfilled, int Backordered);

public class FulfillmentService : IFulfillmentService
{
    private readonly IDbContextFactory<EcommerceDbContext> _factory;
    private readonly BurstPlanner _planner;
    private readonly ConcurrentDictionary<string, int> _skuToProductId;

    public FulfillmentService(IDbContextFactory<EcommerceDbContext> factory, BurstPlanner planner)
    {
        _factory = factory;
        _planner = planner;

        using var db = _factory.CreateDbContext();

        _skuToProductId = new ConcurrentDictionary<string, int>(
            db.Products.ToDictionary(p => p.Sku, p => p.Id)
        );
    }

    public int ResolveProductId(string sku)
    {
        try
        {
            return _skuToProductId[sku];
        }
        catch (KeyNotFoundException)
        {
            throw new UnknownSkuException(sku);
        }
    }

    public async Task<FulfillmentResult> FulfillOneAsync(int orderId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Each order fulfillment gets its own transaction.
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == orderId, ct);

        if (order.Status != OrderStatus.Pending)
        {
            return FulfillmentResult.Backordered;
        }

        var requested = order.Items.ToDictionary(i => i.ProductId, i => i.Quantity);

        foreach (OrderItem item in order.Items)
        {
            ProductInventory inv = await db.Inventory
                .FirstAsync(i => i.ProductId == item.ProductId, ct);

            if (inv.CurrentStock < item.Quantity)
            {
                order.Status = OrderStatus.Backordered;

                db.OrderTracking.Add(new OrderTracking
                {
                    OrderId = orderId,
                    Status = OrderStatus.Backordered,
                    Notes = "Insufficient stock"
                });

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                Log.Warning("Backordered {OrderId}: insufficient stock", orderId);

                return FulfillmentResult.Backordered;
            }

            inv.CurrentStock -= item.Quantity;
        }

        order.Status = OrderStatus.Delivered;
        order.CompletedUtc = DateTime.UtcNow;

        db.OrderTracking.Add(new OrderTracking
        {
            OrderId = orderId,
            Status = OrderStatus.Delivered,
            Notes = "Order fulfilled and delivered"
        });

        if (!await SaveWithRetryAsync(db, requested, ct))
        {
            db.ChangeTracker.Clear();

            Order staleOrder = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
            staleOrder.Status = OrderStatus.Backordered;

            db.OrderTracking.Add(new OrderTracking
            {
                OrderId = orderId,
                Status = OrderStatus.Backordered,
                Notes = "Backordered after inventory retry"
            });

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            Log.Warning("Backordered order {OrderId} after concurrency retry", orderId);

            return FulfillmentResult.Backordered;
        }

        await tx.CommitAsync(ct);

        Log.Information(
            "Fulfilled order {OrderId} with {ItemCount} items",
            orderId,
            order.Items.Count
        );

        return FulfillmentResult.Fulfilled;
    }

    private static async Task<bool> SaveWithRetryAsync(
        EcommerceDbContext db,
        IReadOnlyDictionary<int, int> requestedByProductId,
        CancellationToken ct)
    {
        while (true)
        {
            try
            {
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Warning("Inventory concurrency conflict. Retrying save.");

                foreach (var entry in ex.Entries)
                {
                    var current = await entry.GetDatabaseValuesAsync(ct);

                    if (current is null)
                    {
                        return false;
                    }

                    entry.OriginalValues.SetValues(current);

                    if (entry.Entity is ProductInventory inv)
                    {
                        int freshStock = current.GetValue<int>(
                            nameof(ProductInventory.CurrentStock)
                        );

                        int requestedAmount = requestedByProductId[inv.ProductId];

                        if (freshStock < requestedAmount)
                        {
                            return false;
                        }

                        inv.CurrentStock = freshStock - requestedAmount;
                    }
                }
            }
        }
    }

    public async Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct)
    {
        List<int> idList = orderIds.ToList();

        List<Order> orders;

        await using (var db = await _factory.CreateDbContextAsync(ct))
        {
            orders = await db.Orders
                .Where(o => idList.Contains(o.Id))
                .ToListAsync(ct);
        }

        var planned = _planner.OrderByPriority(orders);

        // Each task calls FulfillOneAsync, and FulfillOneAsync creates its own DbContext.
        var tasks = planned.Select(id => FulfillOneAsync(id, ct));

        var results = await Task.WhenAll(tasks);

        return new BurstResult(
            Fulfilled: results.Count(r => r == FulfillmentResult.Fulfilled),
            Backordered: results.Count(r => r == FulfillmentResult.Backordered)
        );
    }

    public decimal ResolveProductPrice(string sku)
    {
        using var db = _factory.CreateDbContext();

        return db.Products
            .Where(p => p.Sku == sku)
            .Select(p => p.Price)
            .First();
    }
}