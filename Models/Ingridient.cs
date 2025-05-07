using System.ComponentModel.DataAnnotations;

namespace CocktailService.Models;

public class Ingredient
{
    [Key]
    public Guid IngredientId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string NormalizedName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Type { get; set; }

    public bool? IsAlcoholic { get; set; }

    public decimal? Abv { get; set; }
}