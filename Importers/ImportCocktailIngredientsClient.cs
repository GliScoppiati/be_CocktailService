using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Importers;

public class ImportCocktailIngredientsClient
{
    private readonly HttpClient _httpClient;

    public ImportCocktailIngredientsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // DTO di importazione
    public class ImportCocktailIngredientDto
    {
        public Guid CocktailId { get; set; }
        public Guid IngredientId { get; set; }
        public string? OriginalMeasure { get; set; }
        public decimal? QuantityValue { get; set; }
        public string? QuantityUnit { get; set; }
    }

    // Recupera tutti i Cocktail-Ingredient
    public async Task<List<ImportCocktailIngredientDto>> GetAllCocktailIngredientsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<ImportCocktailIngredientDto>>("/cocktails/ingredients");

        return result ?? new List<ImportCocktailIngredientDto>();
    }
}
