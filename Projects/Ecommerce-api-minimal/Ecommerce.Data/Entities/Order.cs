using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;

// This entity represents a customer's order. It stores the customer, priority, status, creation date, 
// completion date, the products being purchased, and the tracking history.
public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = default!;

    public Priority Priority { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedUtc { get; set; }

    // Every order contains one or more items
    public List<OrderItem> Items { get; set; } = new();

    // Every order has a tracking history
    public List<OrderTracking> TrackingHistory { get; set; } = new();
}