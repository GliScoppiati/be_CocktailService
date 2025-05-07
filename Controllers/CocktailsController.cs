using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail")]
public class CocktailsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiCocktailsClient _cocktailsClient;

    public CocktailsController(CocktailDbContext db, ApiCocktailsClient cocktailsClient)
    {
        _db = db;
        _cocktailsClient = cocktailsClient;
    }

    // ✅ IMPORTA cocktail da CocktailImportService (solo ADMIN)
    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<IActionResult> Import()
    {
        var imported = await _cocktailsClient.GetCocktailsAsync();

        foreach (var c in imported)
        {
            var exists = await _db.Cocktails.AnyAsync(x => x.CocktailId == c.CocktailId);
            if (!exists)
            {
                _db.Cocktails.Add(new Cocktail
                {
                    CocktailId = c.CocktailId,
                    Name = c.Name,
                    Category = c.Category ?? "",
                    Glass = c.Glass ?? "",
                    IsAlcoholic = c.IsAlcoholic,
                    Instructions = c.Instructions ?? "",
                    ImageUrl = c.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "import"
                });
            }
        }

        await _db.SaveChangesAsync();

        return Ok("Import completed");
    }

    // ✅ LISTA cocktail (pubblico)
    [HttpGet]
    public async Task<ActionResult<List<Cocktail>>> GetAll()
    {
        var cocktails = await _db.Cocktails
            .OrderBy(x => x.Name)
            .ToListAsync();

        return cocktails;
    }
}
