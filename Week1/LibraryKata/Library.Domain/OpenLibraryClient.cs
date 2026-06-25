using System.Text.Json;
using LibraryKata;
using LibraryKata.Domain;
using Serilog;

public class OpenLibraryClient
{
    // We are going to create and use ONE HTTPClient for the entire process
    // If you use one per call, you are going to leak sockets - and eventually trigger a SocketException

    private static readonly HttpClient client = new();

    public async Task<LibraryItem?> FetchIsbnAsync(string isbn)
    {
        string url = $"https://openlibrary.org/search.json?q=isbn:{isbn}&fields=title,author_name&limit=1";

        try
        {
            string jsonResponse = await client.GetStringAsync(url);

            //return JsonSerializer.Deserialize<>(jsonResponse);
            return Parse(jsonResponse);

        }
        catch (HttpRequestException ex)
        {
            Log.Warning("Network fetch failed for {Isbn} : {Message}", isbn, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Log.Warning("FetchByIsbnAsync failed: {Message}", ex.Message);
            return null;
        }
    }

    // We are going to write our own parsing logic
    public static LibraryItem? Parse(string json)
    {
        // The Search API within OpenLibrary returns a JSON object, and inside that object, amoung other fields -
        // is a "docs" array. If we find the book we want based on its isbn that searched for, its inside that array
        Dictionary<string, JsonElement>? resp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (resp is null || !resp.TryGetValue("docs", out JsonElement docs) || docs.GetArrayLength() == 0)
        {
            return null;
        }

        JsonElement foundBook = docs[0];

        string title = foundBook.GetProperty("title").GetString() ?? "Untitled";

        string author = "Unknown";

        if(foundBook.TryGetProperty("author_name", out JsonElement authors) && authors.GetArrayLength() > 0)
        {
            author = authors[0].GetString() ?? "Unknown";
        }

        return LibraryItemFactory.Create(ItemKind.Book,title,author);
    }
}