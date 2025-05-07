using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;

namespace CocktailService.Controllers;

[ApiController]
[Route("ingredients-map")]
public class CocktailIngredientsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiCocktailIngredientsClient _cocktailIngredientsClient;

    public CocktailIngredientsController(CocktailDbContext db, ApiCocktailIngredientsClient cocktailIngredientsClient)
    {
        _db = db;
        _cocktailIngredientsClient = cocktailIngredientsClient;
    }

    // ✅ IMPORTA CocktailIngredient da CocktailImportService (SOLO ADMIN)
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

        return Ok("Import completed.");
    }

    // ✅ LISTA CocktailIngredient (pubblico → serve per ricostruire i cocktail e le dosi)
    [HttpGet]
    public async Task<ActionResult<List<CocktailIngredient>>> GetAll()
    {
        var ingredientsMap = await _db.CocktailIngredients
            .OrderBy(x => x.CocktailId)
            .ToListAsync();

        return ingredientsMap;
    }
}
