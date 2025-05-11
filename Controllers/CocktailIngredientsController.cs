using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;
using CocktailService.Services;

namespace CocktailService.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrService")]
[Route("cocktail/ingredients-map")]
public class CocktailIngredientsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiCocktailIngredientsClient _cocktailIngredientsClient;
    private readonly SearchSyncClient  _sync;          

    public CocktailIngredientsController(CocktailDbContext db, ApiCocktailIngredientsClient cocktailIngredientsClient, SearchSyncClient sync)
    {
        _db = db;
        _cocktailIngredientsClient = cocktailIngredientsClient;
        _sync = sync;
    }

    // ✅ IMPORT CocktailIngredients → /ingredients-map/import e /cocktail/ingredients/import
    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<IActionResult> Import()
    {
        var imported = await _cocktailIngredientsClient.GetCocktailIngredientsAsync();

        foreach (var ci in imported)
        {
            var exists = await _db.CocktailIngredients.AnyAsync(x =>
                x.CocktailId == ci.CocktailId &&
                x.IngredientId == ci.IngredientId);
            if (!exists)
            {
                _db.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailIngredientId = ci.CocktailIngredientId,
                    CocktailId = ci.CocktailId,
                    IngredientId = ci.IngredientId,
                    OriginalMeasure = ci.OriginalMeasure,
                    QuantityValue = ci.QuantityValue,
                    QuantityUnit = ci.QuantityUnit
                });
            }
        }

        await _db.SaveChangesAsync();
        _ = _sync.TriggerReloadAsync();
        return Ok(new
        {
            message = "Cocktail-ingredient map import completed.",
            importedCount = _db.CocktailIngredients.Count(), // totale attuale dopo import
            importedAt = DateTime.UtcNow
        });
    }

    // ✅ LISTA CocktailIngredients → /ingredients-map e /cocktail/ingredients
    [HttpGet]
    [Authorize(Policy = "AdminOrService")]
    public async Task<ActionResult<List<CocktailIngredient>>> GetAll()
    {
        var ingredientsMap = await _db.CocktailIngredients
            .OrderBy(x => x.CocktailId)
            .ToListAsync();

        return ingredientsMap;
    }
}
