using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Data.Entities;

// An order can contain multiple products, so each product is stored as an OrderItem. 
// It keeps the quantity and the unit price at the moment the order was placed.

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = default!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = default!;

    public int Quantity { get; set; }
    [Precision(10, 2)]
    public decimal UnitPrice { get; set; }
}