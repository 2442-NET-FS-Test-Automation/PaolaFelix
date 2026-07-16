using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.Data.Entities;
// This entity stores customer information such as name and email. 
// Each customer can place multiple orders
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

