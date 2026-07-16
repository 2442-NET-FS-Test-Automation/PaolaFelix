using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Data.Entities;
// This entity represents the products sold in the store. 
// It contains information like SKU, name, description, size, color, and price.
public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ProductSize Size { get; set; }
    public string Color { get; set; } = default!;
    [Precision(10,2)]
    public decimal Price { get; set; }
    // One product has one inventory record
    public ProductInventory? Inventory { get; set; }
}