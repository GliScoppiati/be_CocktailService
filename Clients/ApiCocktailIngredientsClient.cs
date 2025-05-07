using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiCocktailIngredientsClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCocktailIngredientsClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CocktailIngredient>> GetCocktailIngredientsAsync()
    {
        AddAuthorizationHeader();

        var result = await _http.GetFromJsonAsync<List<CocktailIngredient>>("/cocktails/ingredients");

        return result ?? new List<CocktailIngredient>();
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
        }
    }
}
