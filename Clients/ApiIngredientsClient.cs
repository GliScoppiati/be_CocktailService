using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiIngredientsClient
{
    private readonly HttpClient _http;

    public ApiIngredientsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<Ingredient>>("/ingredients");
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }
}