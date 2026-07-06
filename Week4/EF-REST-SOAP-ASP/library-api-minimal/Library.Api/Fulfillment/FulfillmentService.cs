// This class will hold the business Logic/db retry logic for fulfilling transactions
using Library.Data;
using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Library.Api.Fulfillment;
// ASP.NET's builder (DI container) NEEDS us to provide 2 things when we register a service
// An interface and a concrete implementation. These can both go in the same file

public interface IFulfillmentService
{
    public Task<FulfillmentResult> FulfillOneAsync(int orderId, CancellationToken ct);
    public Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct);
}

// Im going to stick everything about order fulfillment in this file
// Requests are either Fulfilled or Backordered - no other results possible
public enum FulfillmentResult { Fulfilled, Backordered }

// Also going to make a record for the result of a Burst (many orders at the same time)
// records are lightweight custom types that allow for comparison with ==
public record BurstResult(int Fulfilled, int Backordered);

public class FulfillmentService : IFulfillmentService
{
    // ASP.NET manages the creation (and destruction) of all our dependencies across our app
    // If we need a DbContext or DbContextfactory or Logger or any other dependency
    // we DO NOT instantiate one here, we ask for one via the Constructor
    private readonly IDbContextFactory<LibraryDbContext> _factory; // holds my factory
    private readonly BurstPLanner _planner; //holds my BUrstPlanner object

    public FulfillmentService(IDbContextFactory<LibraryDbContext> factory, BurstPLanner planner)
    {
        _factory = factory;
        _planner = planner;
    }

    // This method is going to handle fulfillment - its gonna be a bit long. Which is why we didnt
    // just write all of this Program.cs
    public async Task<FulfillmentResult> FulfillOneAsync(int orderId, CancellationToken ct)
    {
    // First - we need a db context
    await using var db = await _factory.CreateDbContextAsync(ct);

    // Lets grab our order from the database
    // Flow for this - a customer places an order. It hits the order table - we are now fulfilling that order
    var order = await db.Orders.Include(o => o.Lines).FirstAsync(o => o.Id == orderId, ct);

    // Lets create that dictionary with the productId Key and the OrderId value
    // yay for LINQ/Collections namespace
    var requested = order.Lines.ToDictionary(l => l.ProductId, l => l.Quantity);

    // creating a flag for "can i continue fulfillinf this order"
    bool canFulfill = true;

    foreach (OrderLine line in order.Lines)
    {
        // First - grab the current inventory from the db for that product
        InventoryItem inv = await db.Inventory.FirstAsync(i => i.ProductId == line.ProductId, ct);

        // Next - check if we can meet the order
        if (inv.CurrentStock < line.Quantity)
        {
            canFulfill = false;
            break;
        }

        inv.CurrentStock -= line.Quantity; // This write to the INventoryItem table is guarderd by RowVersion
    }
    // assuming we broke out of the foreach and cannot fulfill the order
    if (!canFulfill) // checking for canFulfill == false
    {
        // We cant fulfill this order, its now Backordered
        order.Status = Status.Backordered;

        // Create a new fulfillment event record for this transaction, setting it to backordered
        db.FulfillmentEvents.Add(new FulfillmentEvent { OrderId = orderId, Type = "Backordered"});

        await db.SaveChangesAsync(ct);
        // Log the transaction, using the Serilog structured logging syntax
        Log.Warning("Backordered {OrderId}: insufficient stock", orderId);

        return FulfillmentResult.Backordered;
    }

    // If we make it here we can fulfill that order
    order.Status = Status.Fulfilled;
    order.CompletedUtc = DateTime.UtcNow;
    db.FulfillmentEvents.Add(new FulfillmentEvent {OrderId = orderId, Type = "Fulfilled"});

    // Adding our retry save method
    if (!await SaveWithRetryAsync(db, requested, ct)) // if we enter this if - we lost enough lines
        {// that stock dropped this order was backordered
            db.ChangeTracker.Clear();
            Order staleOrder = await db.Orders.FirstAsync(o => o.Id == orderId, ct);;
            staleOrder.Status = Status.Backordered;
            Log.Warning("Backordered order {OrderId} after concurrency retry", orderId);
            return FulfillmentResult.Backordered;
        }

    await db.SaveChangesAsync(ct);
    Log.Information("Fulfilled order: {OrderId}, {LineCount} lines", orderId, order.Lines.Count);
    return FulfillmentResult.Fulfilled;
    }

    // Lets break the logic for saving with retry (via Rowversion) into its own method
    // just to help keep things staright

    private static async Task<bool> SaveWithRetryAsync(
        LibraryDbContext db, IReadOnlyDictionary<int, int> requestedByProductId, CancellationToken ct)
    {
        // This is that RowVersion Change Tracker entry retry from yesterday
        // Lets set max retries to 3 - by wrapping everything in a loop
        while(true)
        {
        // Our loop as written never exists - it does increment ateempt for us
        // If we retry and fail x amount of time - we will throw an exception manually
            try
            {
                // The DbContext inside this method came from FulfillOneAsync - if it has changes
                // staged to it - we can save them here. Its the sam object.
                await db.SaveChangesAsync(ct);
                return true;
            }
            // We can tell our try catch how many times to handle this exception for us
            // After 3 attempts - we eont enter the catch. It bubbles up to wherever this method
            // was called
            catch (DbUpdateConcurrencyException ex) 
            {
                // Retry logic - remember that change tracker stuff?
                // entry is an EF CoreChange tracker entry
                foreach (var entry in ex.Entries)
                {
                    var current = await entry.GetDatabaseValuesAsync(); // grab the current database values

                    // If some other user deleted the entry out from under us.. we cant save
                    // return false 
                    if(current is null) return false;

                    // Set the OriginalValues bucket on the entry to what they currently are
                    entry.OriginalValues.SetValues(current);

                    if(entry.Entity is InventoryItem inv)
                    {
                        // Grab the current totals fot that items stock
                        int freshValue = current.GetValue<int>(nameof(InventoryItem.CurrentStock));
                        //Dictionary lookup against the dict we passed in
                        int desiredAmount = requestedByProductId[inv.ProductId];

                        // Re-check on the fresh stock - dont blindly trsut it
                        if (freshValue < desiredAmount) return false;
                        inv.CurrentStock = freshValue - desiredAmount;
                    }
                }

            }
        }
    }

    public async Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct)
    {
        // Grabbing all my orderIds
        List<int> idList = orderIds.ToList();
        
        List<Order> orders; // place to store my orders

        // Calling on a dbcontext that we discard after were done
        await using (var db = await _factory.CreateDbContextAsync(ct))
        {   // Look in our db, grab every order that appears in our idLIst
            orders = await db.Orders.Where(o => idList.Contains(o.Id)).ToListAsync();
        }

        // Calling on our planning logic inside BurstPlanner
        // planned contains our expedited/priority first order
        var planned = _planner.OrderByPriority(orders);
        
        // we are just going to piggyback off of FulfilOneAsync - no need to rewrite logic we can just call it again
        var tasks = planned.Select(id => FulfillOneAsync(id, ct)); // each row will get its own dbContext

        // Await here until all tasks in the collection are complete
        var results = await Task.WhenAll(tasks);

        return new BurstResult(
            Fulfilled: results.Count(r => r == FulfillmentResult.Fulfilled),
            Backordered: results.Count(r => r == FulfillmentResult.Backordered)
        );
    }
}



