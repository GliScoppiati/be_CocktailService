using CocktailService.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailService.Services;

public class CocktailManager
{
    private readonly CocktailDbContext _db;
    private readonly SearchSyncClient _sync;

    public CocktailManager(CocktailDbContext db, SearchSyncClient sync)
    {
        _db = db;
        _sync = sync;
    }

    public async Task<Guid> CreateCocktailAsync(ApprovedCocktailDto dto, string userId)
    {
        var cocktail = new Cocktail
        {
            CocktailId = Guid.NewGuid(),
            Name = dto.Name,
            Category = dto.Category ?? "",
            Glass = dto.Glass ?? "",
            Instructions = dto.Instructions ?? "",
            ImageUrl = dto.ImageUrl,
            IsAlcoholic = dto.IsAlcoholic,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _db.Cocktails.Add(cocktail);

        await HandleIngredientsAsync(dto.Ingredients, cocktail.CocktailId);

        await _db.SaveChangesAsync();
        _ = _sync.TriggerReloadAsync();

        return cocktail.CocktailId;
    }

    public async Task<CocktailDto?> GetCocktailByIdAsync(Guid id)
    {
        return await _db.Cocktails
            .Where(c => c.CocktailId == id)
            .Select(c => new CocktailDto
            {
                CocktailId = c.CocktailId,
                Name = c.Name,
                Category = c.Category,
                Glass = c.Glass,
                IsAlcoholic = c.IsAlcoholic,
                Instructions = c.Instructions,
                ImageUrl = c.ImageUrl,
                Ingredients = c.CocktailIngredients.Select(ci => new CocktailIngredientDto
                {
                    IngredientId = ci.IngredientId,
                    IngredientName = ci.Ingredient.Name,
                    OriginalMeasure = ci.OriginalMeasure,
                    QuantityValue = ci.QuantityValue,
                    QuantityUnit = ci.QuantityUnit
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateCocktailAsync(Guid id, ApprovedCocktailDto dto)
    {
        var cocktail = await _db.Cocktails.FirstOrDefaultAsync(c => c.CocktailId == id);
        if (cocktail == null) return false;

        cocktail.Name = dto.Name;
        cocktail.Category = dto.Category ?? "";
        cocktail.Glass = dto.Glass ?? "";
        cocktail.Instructions = dto.Instructions ?? "";
        cocktail.ImageUrl = dto.ImageUrl;
        cocktail.IsAlcoholic = dto.IsAlcoholic;

        // rimuovi vecchi ingredienti
        var oldIngredients = await _db.CocktailIngredients
            .Where(ci => ci.CocktailId == id)
            .ToListAsync();

        _db.CocktailIngredients.RemoveRange(oldIngredients);

        await HandleIngredientsAsync(dto.Ingredients, id);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCocktailAsync(Guid id)
    {
        var cocktail = await _db.Cocktails.FirstOrDefaultAsync(c => c.CocktailId == id);
        if (cocktail == null) return false;

        var ingredients = await _db.CocktailIngredients
            .Where(ci => ci.CocktailId == id)
            .ToListAsync();

        _db.CocktailIngredients.RemoveRange(ingredients);
        _db.Cocktails.Remove(cocktail);

        await _db.SaveChangesAsync();
        return true;
    }

    private async Task HandleIngredientsAsync(List<ApprovedIngredientDto> ingredients, Guid cocktailId)
    {
        foreach (var ingDto in ingredients)
        {
            var normalized = Normalize(ingDto.ProposedName);

            var ingredient = await _db.Ingredients
                .FirstOrDefaultAsync(i => i.NormalizedName == normalized);

            if (ingredient == null)
            {
                ingredient = new Ingredient
                {
                    IngredientId = Guid.NewGuid(),
                    Name = ingDto.ProposedName,
                    NormalizedName = normalized
                };
                _db.Ingredients.Add(ingredient);
            }

            _db.CocktailIngredients.Add(new CocktailIngredient
            {
                CocktailIngredientId = Guid.NewGuid(),
                CocktailId = cocktailId,
                IngredientId = ingredient.IngredientId,
                OriginalMeasure = ingDto.Quantity,
                QuantityValue = null,
                QuantityUnit = null
            });
        }
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
}
