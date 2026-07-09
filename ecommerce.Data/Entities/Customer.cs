using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;

[Table("Customer")]

public class Customer
{
    public int Id {get; set;}
    
    [Required, MaxLength(100)]
    public string Name {get; set; } = default!;

    [Required]
    public string Email {get; set; } = default!;
    // One customer can place many orders
    public List<Order> Orders {get; set; } = new(); 
}

