namespace ecommerce.Data.Entities;

// Represents the current state of an order during fulfillment.
public enum OrderStatus
{
    Pending = 0, // Order has been created but has not been processed yet.
    Fulfilled = 1, // Order was successfully fulfilled and inventory was available.
    Backordered = 2  // Order could not be completed because there was not enough stock.
}