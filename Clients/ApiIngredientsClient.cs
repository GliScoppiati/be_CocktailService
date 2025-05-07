using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiIngredientsClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiIngredientsClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        AddAuthorizationHeader();

        var result = await _http.GetFromJsonAsync<List<Ingredient>>("/ingredients");

        return result ?? new List<Ingredient>();
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
