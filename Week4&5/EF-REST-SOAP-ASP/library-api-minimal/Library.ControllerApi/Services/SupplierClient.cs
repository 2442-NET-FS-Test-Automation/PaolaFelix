namespace Library.ControllerApi.Services;

public class SupplierClient : ISupplierClient
{
    // This class will call an outside API using HTTP Client
    private readonly HttpClient _http;

    public SupplierClient(HttpClient http)
    {
        _http = http;
    }

    // Record to represent the response "shape" of that outside API
    private record SupplierProduct(int id, string Title, decimal Price);

    // This method sends a GET to a training API called dummyjson
    // GET https://dummyjson.com/products/{id} -> this is live
    public async Task<decimal?> GetListPriceAsync(string sku)
    {
        // Lets pretend we are grabbing the "wholesale price" of our products from the supplier

        // Trimming our SKUs
        var digits = new string(sku.Where(char.IsDigit).ToArray()); // "Bk-001" -> "001"

        // Check to make sure we dont have a null in digits
        if(!int.TryParse(digits, out var id)) return null; // If our string is empty just return null

        // Appending the rest of the URL to the base URL we set up with builder.Services
        var product = await _http.GetFromJsonAsync<SupplierProduct>($"products/{id}");

        return product?.Price;
    }
}