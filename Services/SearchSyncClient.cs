using System.Net.Http;
using System.Threading.Tasks;

namespace CocktailService.Services;

public class SearchSyncClient
{
    private readonly HttpClient _http;

    public SearchSyncClient(HttpClient http) => _http = http;

    public Task TriggerReloadAsync()
        => _http.PostAsync("/cocktails/reload/now", null);   // body vuoto
}