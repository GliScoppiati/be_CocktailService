using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CocktailService.Models;

public class Cocktail
{
    [Key]
    public Guid CocktailId { get; set; }

    public string? OrigId { get; set; } // OrigId se importato, null se user

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Instructions { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Glass { get; set; } = string.Empty;

    public bool IsAlcoholic { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    public CocktailSource SourceType { get; set; } = CocktailSource.Imported;

    public string CreatedBy { get; set; } = "import";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
}
