using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;

namespace CocktailService.Controllers;

[ApiController]
[Route("ingredients")]
public class IngredientsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiIngredientsClient _ingredientsClient;

    public IngredientsController(CocktailDbContext db, ApiIngredientsClient ingredientsClient)
    {
        _db = db;
        _ingredientsClient = ingredientsClient;
    }

    // ✅ IMPORTA ingredienti da CocktailImportService (solo ADMIN)
    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<IActionResult> Import()
    {
        var imported = await _ingredientsClient.GetIngredientsAsync();

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
            }
        }

        await _db.SaveChangesAsync();

        return Ok("Import completed");
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
