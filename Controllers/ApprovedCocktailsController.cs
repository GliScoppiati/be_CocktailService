using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail/approved")]
public class ApprovedCocktailsController : ControllerBase
{
    private readonly CocktailDbContext _db;

    public ApprovedCocktailsController(CocktailDbContext db)
    {
        _db = db;
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportApprovedCocktail([FromBody] ApprovedCocktailDto dto)
    {
        if (dto == null)
            return BadRequest("Payload non valido.");

        if (dto.Ingredients == null)
            dto.Ingredients = new List<ApprovedIngredientDto>();

        var normalizedCocktailName = Normalize(dto.Name);

        var cocktails = await _db.Cocktails.ToListAsync();

        var cocktailExists = cocktails
            .Any(c => Normalize(c.Name) == normalizedCocktailName);

        if (cocktailExists)
            return BadRequest($"A cocktail with the name '{dto.Name}' already exists.");

        var cocktail = new Cocktail
        {
            CocktailId = Guid.NewGuid(),
            Name = dto.Name,
            Category = dto.Category ?? string.Empty,
            Glass = dto.Glass ?? string.Empty,
            Instructions = dto.Instructions ?? string.Empty,
            ImageUrl = dto.ImageUrl,
            IsAlcoholic = dto.IsAlcoholic,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "ApprovedSubmission"
        };

        _db.Cocktails.Add(cocktail);

        foreach (var ingDto in dto.Ingredients)
        {
            Ingredient? ingredient = null;

            // ➡️ 1. Normalizza il nome proposto
            var normalizedIngredientName = Normalize(ingDto.ProposedName);

            // ➡️ 2. Prova a trovare un ingrediente già esistente con quel NormalizedName
            ingredient = await _db.Ingredients
                .FirstOrDefaultAsync(i => i.NormalizedName == normalizedIngredientName);

            if (ingredient == null)
            {
                // ➡️ 3. Se non esiste, creane uno nuovo
                ingredient = new Ingredient
                {
                    IngredientId = Guid.NewGuid(),
                    Name = ingDto.ProposedName,
                    NormalizedName = normalizedIngredientName
                };

                _db.Ingredients.Add(ingredient);
            }

            // ➡️ 4. Mappa il cocktail con l'ingrediente
            _db.CocktailIngredients.Add(new CocktailIngredient
            {
                CocktailIngredientId = Guid.NewGuid(),
                CocktailId = cocktail.CocktailId,
                IngredientId = ingredient.IngredientId,
                OriginalMeasure = ingDto.Quantity,
                QuantityUnit = null,
                QuantityValue = null
            });
        }

        await _db.SaveChangesAsync();

        return Ok("Approved cocktail imported successfully.");
    }
}

public class ApprovedCocktailDto
{
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Glass { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAlcoholic { get; set; }
    public string? ImageUrl { get; set; }
    public List<ApprovedIngredientDto> Ingredients { get; set; } = new();
}

public class ApprovedIngredientDto
{
    public Guid IngredientId { get; set; } = Guid.Empty;
    public string ProposedName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
