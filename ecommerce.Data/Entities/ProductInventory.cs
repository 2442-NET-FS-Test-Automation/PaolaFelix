using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;

public class ProductInventory
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = default!;

    public int CurrentStock { get; set; }

    // Used by EF Core for concurrency checking
    public byte[] RowVersion { get; set; } = default!;
}