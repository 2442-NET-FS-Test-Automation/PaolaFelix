using ecommerce.Data.Entities;

namespace ecommerce.Api.Fulfillment;

// Responsible for creating new Order objects.
// It builds orders with the correct priority, status, and order items.
public class OrderFactory
{
    private readonly IFulfillmentService _fs;

    public OrderFactory(IFulfillmentService fulfillment)
    {
        _fs = fulfillment;
    }

    public Order CreateOrder(string kind, int customerId, IEnumerable<(string sku, int qty)> items)
    {
        switch (kind.ToLower())
        {
            case "normal":
                return BuildOrder(Priority.Normal, customerId, items);

            case "expedited":
                return BuildOrder(Priority.Expedited, customerId, items);

            default:
                throw new ArgumentException($"Unknown order type: {kind}");
        }
    }

    private Order BuildOrder(
        Priority priority,
        int customerId,
        IEnumerable<(string sku, int qty)> items)
    {
        return new Order
        {
            CustomerId = customerId,
            Priority = priority,
            Status = OrderStatus.Pending,

            Items = items.Select(i => new OrderItem
            {
                ProductId = _fs.ResolveProductId(i.sku),
                Quantity = i.qty,
                UnitPrice = _fs.ResolveProductPrice(i.sku)
            }).ToList()
        };
    }
}