using CocktailService.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailService.Importers;

public class CocktailImportSyncService
{
    private readonly CocktailDbContext _db;
    private readonly ImportIngredientsClient _ingredientsClient;
    private readonly ImportCocktailsClient _cocktailsClient;

    public CocktailImportSyncService(
        CocktailDbContext db,
        ImportIngredientsClient ingredientsClient,
        ImportCocktailsClient cocktailsClient)
    {
        _db = db;
        _ingredientsClient = ingredientsClient;
        _cocktailsClient = cocktailsClient;
    }

    public async Task ImportAsync()
    {
        // Import Ingredients
        var ingredients = await _ingredientsClient.GetIngredientsAsync();

        foreach (var ingredient in ingredients)
        {
            var exists = await _db.Ingredients.AnyAsync(i => i.IngredientId == ingredient.IngredientId);
            if (!exists)
            {
                _db.Ingredients.Add(ingredient);
            }
        }

        await _db.SaveChangesAsync();

        // Import Cocktails
        var cocktails = await _cocktailsClient.GetCocktailsAsync();

        foreach (var cocktailDto in cocktails)
        {
            var exists = await _db.Cocktails.AnyAsync(c => c.OrigId == cocktailDto.OrigId);
            if (exists)
                continue;

            var cocktail = new Cocktail
            {
                CocktailId = Guid.NewGuid(),
                OrigId = cocktailDto.OrigId,
                Name = cocktailDto.Name,
                Category = cocktailDto.Category,
                IsAlcoholic = cocktailDto.IsAlcoholic,
                Glass = cocktailDto.Glass,
                Instructions = cocktailDto.Instructions,
                ImageUrl = cocktailDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "IMPORT"
            };

            foreach (var ing in cocktailDto.Ingredients)
            {
                cocktail.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailId = cocktail.CocktailId,
                    IngredientId = ing.IngredientId,
                    OriginalMeasure = ing.OriginalMeasure,
                    QuantityValue = ing.QuantityValue,
                    QuantityUnit = ing.QuantityUnit
                });
            }

            _db.Cocktails.Add(cocktail);
        }

        await _db.SaveChangesAsync();
    }
}
