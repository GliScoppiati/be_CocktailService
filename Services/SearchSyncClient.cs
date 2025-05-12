using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CocktailService.Services;

public class SearchSyncClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SearchSyncClient> _logger;

    public SearchSyncClient(HttpClient http, ILogger<SearchSyncClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task TriggerReloadAsync()
    {
        try
        {
            _logger.LogInformation("[CocktailService] 🔁 Inizio sincronizzazione cocktail con SearchService...");

            var response = await _http.PostAsync("/cocktails/reload/now", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[CocktailService] ✅ Sincronizzazione completata con successo.");
            }
            else
            {
                _logger.LogWarning("[CocktailService] ⚠️ Sincronizzazione fallita. StatusCode: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CocktailService] ❌ Errore durante la sincronizzazione con SearchService.");
        }
    }
}
