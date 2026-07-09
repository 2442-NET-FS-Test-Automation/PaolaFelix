using ecommerce.Data.Entities;
using ecommerce.Data;
using ecommerce.Api.Fulfillment;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Api.Fulfillment;

public class BurstPlanner
{
    // This method decides the order in which pending orders should be processed.
    // Expedited orders get processed first. If two orders have the same priority,
    // the older order gets processed first.
    public IReadOnlyList<int> OrderByPriority(IEnumerable<Order> orders)
    {
        return orders
            .OrderBy(o => o.Priority == Priority.Expedited ? 0 : 1)
            .ThenBy(o => o.CreatedUtc)
            .Select(o => o.Id)
            .ToList();
    }
}