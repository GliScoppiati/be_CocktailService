using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocktailService.Models;

public class CocktailIngredient
{
    [Key]
    public Guid CocktailIngredientId { get; set; }

    [Required]
    public Guid CocktailId { get; set; }

    [ForeignKey(nameof(CocktailId))]
    public Cocktail Cocktail { get; set; } = null!;

    [Required]
    public Guid IngredientId { get; set; }

    [ForeignKey(nameof(IngredientId))]
    public Ingredient Ingredient { get; set; } = null!;

    public decimal? QuantityValue { get; set; }

    public string? QuantityUnit { get; set; }

    public string? OriginalMeasure { get; set; }
}
