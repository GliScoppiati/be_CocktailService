using System.Net.Http;
using System.Threading.Tasks;

namespace CocktailService.Services;

public class SearchSyncClient
{
    private readonly HttpClient _http;

    public SearchSyncClient(HttpClient http) => _http = http;

    public Task TriggerReloadAsync()
    {
        Console.WriteLine("🔁 Chiamata HTTP a /cocktails/reload/now");
        return _http.PostAsync("/cocktails/reload/now", null);
    }
}