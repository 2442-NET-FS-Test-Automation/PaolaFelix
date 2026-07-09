namespace ecommerce.Data.Entities;

public enum OrderStatus
{
    Pending = 0,
    Picked = 1,
    Packed = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Backordered = 6
}