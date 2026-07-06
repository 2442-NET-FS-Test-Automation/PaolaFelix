namespace Library.Data.Entities;

public class FulfillmentEvent
{
    public int Id {get; set;}
    public int OrderId {get; set; }
    // = default is something were doing for EF Core. If we were to make this nullable wed
    // satisfy the compiler - but what if I DONT want the database column to allow a null?
    // =default! lets me shove some default value (varies per type) into the property on creation
    public string Type {get; set;} = default!; 
    public DateTime FulfilledAtUtc {get; set; } = DateTime.UtcNow;

}