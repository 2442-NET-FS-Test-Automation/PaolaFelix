using Library.Data.Entities;
using Library.Api.Fulfillment;

public class BurstPLanner
{
    // Method to plan fulfillment order
    public IReadOnlyList<int> OrderByPriority(IEnumerable<Order> orders)
    {
        // We could make our own custom implementation on this - we wont
        // We can use a PriorityQueue - allows for FIFO processing with priority taken into account
        // First int = OrderId, Second int = priority
        // we are going to use lower number = higher priority
        PriorityQueue<int, int> pq = new PriorityQueue<int, int>();

        foreach (Order o in orders)
            // Enqueue each order, if its priority is expedited, give it a 0 value, if normal give it 1
            pq.Enqueue(o.Id, o.Priority == Priority.Expedited ? 0 : 1);

        // This list will hold everything we want to process already in order to pass to our other methods
        var orderedByPriority = new List<int>();

        // While out PriorityQueue has stuff in it - loop and add those things in the order they exit
        // to our orderedBYPriority list - uses out params
        while (pq.TryDequeue(out int id, out _))
        {
            orderedByPriority.Add(id);
        }

        return orderedByPriority; // expedited ids should be first in the list
    }
}