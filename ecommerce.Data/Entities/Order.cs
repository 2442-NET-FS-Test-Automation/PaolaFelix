using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;

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