using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;
// This entity stores the inventory for each product. 
// It keeps track of the current stock, and the RowVersion property 
// is used for optimistic concurrency so multiple orders can't update 
// the same inventory record at the same time.
public class ProductInventory
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = default!;

    public int CurrentStock { get; set; }

    // Used by EF Core for concurrency checking
    public byte[] RowVersion { get; set; } = default!;
}