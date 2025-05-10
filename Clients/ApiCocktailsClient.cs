using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiCocktailsClient
{
    private readonly HttpClient _http;

    public ApiCocktailsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Cocktail>> GetCocktailsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<Cocktail>>("/cocktails");
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }
}
