using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;
using CocktailService.Services;

namespace CocktailService.Controllers;

[ApiController]
[Authorize]
[Route("cocktail/ingredients")]
public class IngredientsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiIngredientsClient _ingredientsClient;
    private readonly SearchSyncClient  _sync; 

    public IngredientsController(CocktailDbContext db, ApiIngredientsClient ingredientsClient, SearchSyncClient sync)
    {
        _db = db;
        _ingredientsClient = ingredientsClient;
        _sync = sync;
    }

    // ✅ IMPORTA ingredienti da CocktailImportService (solo ADMIN)
    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<IActionResult> Import()
    {
        var imported = await _ingredientsClient.GetIngredientsAsync();
        int importedCount = 0;

        foreach (var i in imported)
        {
            var exists = await _db.Ingredients.AnyAsync(x => x.IngredientId == i.IngredientId);
            if (!exists)
            {
                _db.Ingredients.Add(new Ingredient
                {
                    IngredientId = i.IngredientId,
                    Name = i.Name,
                    NormalizedName = i.NormalizedName
                });
                importedCount++;
            }
        }

        await _db.SaveChangesAsync();
        _ = _sync.TriggerReloadAsync();

        return Ok(new
        {
            message = "Ingredients import completed",
            importedCount = importedCount,
            importedAt = DateTime.UtcNow
        });
    }

    // ✅ LISTA tutti gli ingredienti disponibili (pubblico)
    [HttpGet]
    public async Task<ActionResult<List<Ingredient>>> GetAll()
    {
        var ingredients = await _db.Ingredients
            .OrderBy(x => x.Name)
            .ToListAsync();

        return ingredients;
    }
}
