using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CocktailService.Models;
using CocktailService.Services;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail/approved")]
public class ApprovedCocktailsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly SearchSyncClient _sync;

    public ApprovedCocktailsController(CocktailDbContext db, SearchSyncClient sync)
    {
        _db = db;
        _sync = sync;
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

        var userId = dto.CreatedByUserId ?? "Unknown";

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
            CreatedBy = userId
        };

        _db.Cocktails.Add(cocktail);

        foreach (var ingDto in dto.Ingredients)
        {
            Ingredient? ingredient = null;

            var normalizedIngredientName = Normalize(ingDto.ProposedName);

            ingredient = await _db.Ingredients
                .FirstOrDefaultAsync(i => i.NormalizedName == normalizedIngredientName);

            if (ingredient == null)
            {
                ingredient = new Ingredient
                {
                    IngredientId = Guid.NewGuid(),
                    Name = ingDto.ProposedName,
                    NormalizedName = normalizedIngredientName
                };

                _db.Ingredients.Add(ingredient);
            }

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
        _ = _sync.TriggerReloadAsync();

        return Ok(new
        {
            message = "Approved cocktail imported successfully.",
            cocktailId = cocktail.CocktailId,
            name = cocktail.Name,
            createdAt = cocktail.CreatedAt
        });
    }
}

// public class ApprovedCocktailDto
// {
//     public string Name { get; set; } = string.Empty;
//     public string Instructions { get; set; } = string.Empty;
//     public string Glass { get; set; } = string.Empty;
//     public string Category { get; set; } = string.Empty;
//     public bool IsAlcoholic { get; set; }
//     public string? ImageUrl { get; set; }
//     public string CreatedByUserId { get; set; } = string.Empty;
//     public List<ApprovedIngredientDto> Ingredients { get; set; } = new();
// }

// public class ApprovedIngredientDto
// {
//     public Guid IngredientId { get; set; } = Guid.Empty;
//     public string ProposedName { get; set; } = string.Empty;
//     public string Quantity { get; set; } = string.Empty;
// }
