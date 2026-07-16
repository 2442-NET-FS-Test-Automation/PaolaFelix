using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;
// This entity stores the tracking history of an order. Every time an order changes status,
//  I can record the new status, some notes, and the time it happened.
public class OrderTracking
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = default!;

    public OrderStatus Status { get; set; }

    public string Notes { get; set; } = default!;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}