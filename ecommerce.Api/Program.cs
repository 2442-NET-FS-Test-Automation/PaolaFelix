using ecommerce.Data;
using ecommerce.Data.Entities;
using ecommerce.Api.Fulfillment;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using ecommerce.Api.Exceptions;

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
app.MapGet("/Inventory", async (IInventoryRepository repo) =>
{
    var inventory = await repo.GetAllAsync();

    return inventory.Select(i => new
    {
        i.Id,
        i.ProductId,
        Sku = i.Product.Sku,
        ProductName = i.Product.Name,
        Category = i.Product.Category.Name,
        i.Product.Size,
        i.Product.Color,
        i.Product.Price,
        i.CurrentStock
    });
});

app.Run();

Log.CloseAndFlush();

