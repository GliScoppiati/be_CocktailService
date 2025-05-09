namespace CocktailService.Models;

public class CocktailDto
{
    public Guid CocktailId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Glass { get; set; } = string.Empty;
    public bool IsAlcoholic { get; set; }
    public string? ImageUrl { get; set; }
    public string Instructions { get; set; } = string.Empty;

    public List<CocktailIngredientDto> Ingredients { get; set; } = new();
}
