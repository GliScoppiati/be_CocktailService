using System.Net.Http.Json;
using CocktailService.Models;

namespace CocktailService.Importers;

public class ImportIngredientsClient
{
    private readonly HttpClient _httpClient;

    public ImportIngredientsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        var ingredients = await _httpClient.GetFromJsonAsync<List<IngredientDto>>("/ingredients");

        return ingredients?.Select(dto => new Ingredient
        {
            IngredientId = dto.IngredientId,
            Name = dto.Name,
            NormalizedName = dto.NormalizedName,
            Description = dto.Description,
            Type = dto.Type,
            IsAlcoholic = dto.IsAlcoholic,
            Abv = dto.Abv
        }).ToList() ?? new();
    }

    private class IngredientDto
    {
        public Guid IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public bool? IsAlcoholic { get; set; }
        public decimal? Abv { get; set; }
    }
}
