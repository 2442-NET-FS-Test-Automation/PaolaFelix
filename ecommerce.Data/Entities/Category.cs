using System.ComponentModel.DataAnnotations;

namespace ecommerce.Data.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = default!;

    // One category contains many products
    public List<Product> Products { get; set; } = new();
}