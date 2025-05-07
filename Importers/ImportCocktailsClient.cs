using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Importers;

public class ImportCocktailsClient
{
    private readonly HttpClient _httpClient;

    public ImportCocktailsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CocktailImportDto>> GetCocktailsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CocktailImportDto>>("/cocktails") ?? new();
    }

    public class CocktailImportDto
    {
        public Guid CocktailId { get; set; }
        public string OrigId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsAlcoholic { get; set; }
        public string? Glass { get; set; }
        public string? Instructions { get; set; }
        public string? ImageUrl { get; set; }
        public List<CocktailIngredientImportDto> Ingredients { get; set; } = new();
    }

    public class CocktailIngredientImportDto
    {
        public Guid IngredientId { get; set; }
        public string? OriginalMeasure { get; set; }
        public decimal? QuantityValue { get; set; }
        public string? QuantityUnit { get; set; }
    }
}
