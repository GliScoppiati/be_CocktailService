using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiCocktailIngredientsClient
{
    private readonly HttpClient _http;

    public ApiCocktailIngredientsClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CocktailIngredient>> GetCocktailIngredientsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<CocktailIngredient>>(
                             "/cocktails/ingredients");

            return result ?? new();
        }
        catch
        {
            return new();
        }
    }
}
