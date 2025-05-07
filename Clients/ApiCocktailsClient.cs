using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Clients;

public class ApiCocktailsClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiCocktailsClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<Cocktail>> GetCocktailsAsync()
    {
        AddAuthorizationHeader();

        var result = await _http.GetFromJsonAsync<List<Cocktail>>("/cocktails");

        return result ?? new List<Cocktail>();
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
