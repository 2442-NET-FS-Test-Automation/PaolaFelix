using ecommerce.Data;
using ecommerce.Data.Entities;
using ecommerce.Api.Fulfillment;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using ecommerce.Api.Exceptions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var conn_string = "Server=localhost,1433;Database=EcommerceMinimalDb;User Id=sa;Password=Password1234;TrustServerCertificate=true";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ecommerce-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<EcommerceDbContext>(options => options.UseSqlServer(conn_string),
    ServiceLifetime.Scoped, ServiceLifetime.Singleton
);

builder.Services.AddDbContextFactory<EcommerceDbContext>(options => options.UseSqlServer(conn_string));

// Registered our custom service with the builder
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ISeeder, Seeder>();
builder.Services.AddScoped<BurstPlanner>();
builder.Services.AddScoped<IFulfillmentService, FulfillmentService>();
builder.Services.AddScoped<OrderFactory>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger stuff added to builder
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// App area
var app = builder.Build();

// Swagger stuff added to app
app.UseSwagger();
app.UseSwaggerUI();

// Endpoint area

// Get all items from the inventory
app.MapGet("/inventory", async (IInventoryRepository repo) =>
{
    var inventory = await repo.GetAllAsync();

    return inventory.Select(i => new
    {
        i.Id,
        i.ProductId,
        Sku = i.Product.Sku,
        ProductName = i.Product.Name,
        Category = i.Product.Category?.Name,
        i.Product.Size,
        i.Product.Color,
        i.Product.Price,
        i.CurrentStock
    });
});

// Restock an existing product increment (Safely updates through unchanged repo)
app.MapPatch("/products/restock", async (string sku, int addQuantity, IInventoryRepository repo) =>
{
    if (addQuantity <= 0) return Results.BadRequest("Quantity to add must be greater than zero.");

    // Look up current item details
    var item = await repo.GetInventoryItemBySkuAsync(sku);
    if (item is null)
    {
        // Throw or handle your custom exception cleanly
        throw new UnknownSkuException(sku);
    }

    // Calculate total absolute stock 
    int newStock = item.CurrentStock + addQuantity;

    // Save using your unchanged repository method
    await repo.UpdateStockBySkuAsync(sku, newStock);

    return Results.Ok(new { message = "Stock adjusted successfully", sku, currentStock = newStock });
});

// Remove a product and its inventory completely from the database by SKU
app.MapDelete("/products/{sku}", async (string sku, IInventoryRepository repo) =>
{
    bool deleted = await repo.RemoveBySkuAsync(sku);

    if (!deleted)
    {
        throw new UnknownSkuException(sku);
    }

    return Results.Ok(new { message = $"Product with SKU {sku} was successfully deleted." });
});

// Add a brand new item to your catalog via your unmodified repository method
app.MapPost("/products/create", async (AddProductDto dto, IInventoryRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(dto.Sku)) return Results.BadRequest("SKU is required.");
    if (dto.StartingStock < 0) return Results.BadRequest("Starting stock cannot be negative.");

    var newItem = await repo.AddInventoryItemAsync(
        dto.Sku, dto.Name, dto.Description, dto.CategoryId, 
        dto.Size, dto.Color, dto.Price, dto.StartingStock
    );

    return Results.Created($"/inventory", newItem);
});

// Seed system order variants
app.MapPost("/orders/create", (int n, bool expedited, ISeeder seeder) =>
{
    if (n <= 0) return Results.BadRequest(new { message = "The number of orders must be greater than zero" });

    var ids = seeder.SeedOrders(n, expedited);
    return Results.Ok(new { message = "Test orders created", orderCount = ids.Count });
});

// Process pending orders concurrently 
app.MapPost("/orders/burst", async (
    EcommerceDbContext db,
    IFulfillmentService service,
    IHostApplicationLifetime lifetime) =>
{
    // Gather the pending IDs sorted by priority first 
    var ids = await db.Orders
        .Where(o => o.Status == OrderStatus.Pending)
        .OrderBy(o => o.Priority == Priority.Expedited ? 0 : 1)
        .ThenBy(o => o.CreatedUtc)
        .Select(o => o.Id)
        .ToListAsync();

    if (ids.Count == 0)
    {
        return Results.Ok(new 
        { 
            message = "There are no pending orders", 
            fulfilled = 0, 
            backordered = 0 
        });
    }

    // Call the service and await the aggregated results array directly
    // This uses the Task.WhenAll multi-threading block your trainer wrote!
    BurstResult result = await service.FulfillBurstAsync(ids, lifetime.ApplicationStopping);

    // Return the exact counts immediately in the response body
    return Results.Ok(new
    {
        message = "Burst processing completed",
        totalProcessed = ids.Count,
        fulfilledCount = result.Fulfilled,
        backorderedCount = result.Backordered 
    });
});

app.MapPost("/benchmark", async (int n, IFulfillmentService fs, ISeeder seeder, CancellationToken ct) =>
{
    // Reset everything and create 'n' fresh test orders for the sequential test
    var ids1 = seeder.SeedOrders(n, expedited: false); // or your ResetAndCreateOrders equivalent

    // Run the Sequential test
    var sw1 = Stopwatch.StartNew(); 
    foreach (var id in ids1)
    {
        await fs.FulfillOneAsync(id, ct);
    }
    sw1.Stop();

    // Reset the field completely and seed 'n' fresh orders for the concurrent test
    seeder.ResetInventory(); 
    var ids2 = seeder.SeedOrders(n, expedited: false); 

    // Run the Concurrent test
    var sw2 = Stopwatch.StartNew(); 
    await fs.FulfillBurstAsync(ids2, ct);
    sw2.Stop();

    // Calculate the speedup factor metric for your Target assignment grade
    long seqMs = sw1.ElapsedMilliseconds;
    long conMs = sw2.ElapsedMilliseconds;
    double speedupFactor = conMs > 0 ? (double)seqMs / conMs : 1.0;

    return new
    {
        sequentialMs = seqMs,
        concurrentMs = conMs,
        speedupFactor = $"{speedupFactor:F2}x faster using concurrency"
    };
});

// Top Products by overall fulfillment volume metrics
app.MapGet("/reports/top-products", async (EcommerceDbContext db) =>
{
    var topProducts = await db.OrderItems
        .Where(oi => oi.Order.Status == OrderStatus.Fulfilled)
        .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
        .Select(g => new
        {
            g.Key.ProductId,
            g.Key.Name,
            TotalVolume = g.Sum(oi => oi.Quantity)
        })
        .OrderByDescending(p => p.TotalVolume)
        .ToListAsync();

    return Results.Ok(topProducts);
});

// Top Customers order placement counts
app.MapGet("/reports/top-customers", async (EcommerceDbContext db) =>
{
    var topCustomers = await db.Orders
        .Where(o => o.Status == OrderStatus.Fulfilled)
        .GroupBy(o => o.CustomerId)
        .Select(g => new
        {
            CustomerId = g.Key,
            TotalOrdersFulfilled = g.Count()
        })
        .OrderByDescending(c => c.TotalOrdersFulfilled)
        .ToListAsync();

    return Results.Ok(topCustomers);
});

// Rate metrics showing total breakdown between Fulfilled and Backordered sets
app.MapGet("/reports/fulfillment-rate", async (EcommerceDbContext db) =>
{
    var counts = await db.Orders
        .GroupBy(o => o.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToDictionaryAsync(k => k.Status, v => v.Count);

    counts.TryGetValue(OrderStatus.Fulfilled, out int fulfilled);
    counts.TryGetValue(OrderStatus.Backordered, out int backordered);
    int total = fulfilled + backordered;

    return Results.Ok(new
    {
        TotalProcessed = total,
        FulfilledCount = fulfilled,
        BackorderedCount = backordered,
        FulfillmentRate = total > 0 ? $"{(double)fulfilled / total * 100:F2}%" : "0.00%"
    });
});

// Shows the detailed history of the orders
app.MapGet("/reports/orders-detailed", async (EcommerceDbContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
        .OrderBy(o => o.Status) 
        .ToListAsync();

    return Results.Ok(orders.Select(o => new
    {
        OrderId = o.Id,
        CustomerId = o.CustomerId, 
        Priority = o.Priority.ToString(), 
        Status = o.Status.ToString(),
        CompletedAt = o.CompletedUtc,
        ItemsOrdered = o.Items.Select(item => new
        {
            Sku = item.Product?.Sku ?? "UNKNOWN",
            ProductName = item.Product?.Name ?? "Unknown Product",
            Quantity = item.Quantity,
            PricePaid = item.UnitPrice
        })
    }));
});

// Resets inventory back to hardcoded baseline configurations
app.MapPost("/inventory/reset", (ISeeder seeder) =>
{
    seeder.ResetInventory();
    return Results.Ok("Inventory reset");
});

// Wipes out all orders and tracking history for a fresh baseline
app.MapPost("/orders/reset-all", async (EcommerceDbContext db, ISeeder seeder) =>
{
    // Wipe out child tables first due to Foreign Key constraints
    db.OrderItems.RemoveRange(db.OrderItems);
    if (db.Set<OrderTracking>() != null) 
    {
        db.RemoveRange(db.Set<OrderTracking>());
    }

    // Wipe out parent Orders table
    db.Orders.RemoveRange(db.Orders);
    await db.SaveChangesAsync();

    // Reset SQL Identity Counters back to 1 so order IDs start at 1 again
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Orders', RESEED, 0);");
    await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('OrderItems', RESEED, 0);");
    if (db.Set<OrderTracking>() != null)
    {
         await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('OrderTracking', RESEED, 0);");
    }

    seeder.ResetInventory();

    return Results.Ok(new 
    { 
        message = "System successfully refreshed! All orders wiped, IDs reseeded to 1, and inventory stock restored." 
    });
});

app.Run();

Log.CloseAndFlush();

public record AddProductDto(
    string Sku, string Name, string Description, int CategoryId, 
    ProductSize Size, string Color, decimal Price, int StartingStock
);