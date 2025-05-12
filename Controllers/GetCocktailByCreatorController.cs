using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail")]
public class GetCocktailByCreatorController : ControllerBase
{
    private readonly CocktailDbContext _db;

    public GetCocktailByCreatorController(CocktailDbContext db)
    {
        _db = db;
    }

    // 🔍 Cerca cocktail approvato da username + nome
    // GET /cocktail/by-creator?username=sofi&name=Margarita
    [HttpGet("by-creator")]
    [Authorize]
    public async Task<ActionResult<object>> GetCocktailIdByCreator(
        [FromQuery] string username,
        [FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(name))
            return BadRequest("Username e nome del cocktail sono obbligatori.");

        string normName = name.Trim().ToLowerInvariant();
        string normUser = username.Trim().ToLowerInvariant();

        if (normUser == "anon")
        {
            var cocktails = await _db.Cocktails
                .Where(c => c.Name.ToLower() == normName)
                .Select(c => new
                {
                    cocktailId = c.CocktailId,
                    name       = c.Name,
                    createdBy  = c.CreatedBy
                })
                .ToListAsync();

            if (!cocktails.Any())
                return NotFound("Nessun cocktail trovato con questo nome.");

            return Ok(cocktails);
        }
        else
        {
            var cocktail = await _db.Cocktails
                .Where(c => c.CreatedBy.ToLower() == normUser && c.Name.ToLower() == normName)
                .Select(c => new
                {
                    cocktailId = c.CocktailId,
                    name       = c.Name,
                    createdBy  = c.CreatedBy
                })
                .FirstOrDefaultAsync();

            if (cocktail == null)
                return NotFound("Nessun cocktail trovato per questo autore e nome.");

            return Ok(cocktail);
        }
    }
}
