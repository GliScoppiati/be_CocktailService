namespace CocktailService.Models;

public class CocktailIngredientDto
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string? OriginalMeasure { get; set; }
    public decimal? QuantityValue { get; set; }
    public string? QuantityUnit { get; set; }
}
