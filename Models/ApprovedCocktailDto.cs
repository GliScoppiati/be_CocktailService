using System.ComponentModel.DataAnnotations;

namespace CocktailService.Models;

public class ApprovedCocktailDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;
    public string Glass { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAlcoholic { get; set; }
    public string? ImageUrl { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public List<ApprovedIngredientDto> Ingredients { get; set; } = new();
}

public class ApprovedIngredientDto
{
    public Guid IngredientId { get; set; } = Guid.Empty;
    public string ProposedName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}