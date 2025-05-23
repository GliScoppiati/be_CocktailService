using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Clients;
using CocktailService.Services;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail")]
public class CocktailsController : ControllerBase
{
    private readonly CocktailDbContext _db;
    private readonly ApiCocktailsClient _cocktailsClient;
    private readonly CocktailManager _cocktailManager;
    private readonly SearchSyncClient _sync;

    public CocktailsController(
        CocktailDbContext db,
        ApiCocktailsClient cocktailsClient,
        CocktailManager cocktailManager,
        SearchSyncClient sync)
    {
        _db = db;
        _cocktailsClient = cocktailsClient;
        _cocktailManager = cocktailManager;
        _sync = sync;
    }

    // ✅ IMPORT da CocktailImportService (solo ADMIN)
    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<IActionResult> Import()
    {
        var imported = await _cocktailsClient.GetCocktailsAsync();
        int importedCount = 0;

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
                importedCount++;
            }
        }

        await _db.SaveChangesAsync();
        _ = _sync.TriggerReloadAsync();

        return Ok(new
        {
            message = "Import completed",
            importedCount = importedCount,
            importedAt = DateTime.UtcNow
        });
    }

    // ✅ LISTA cocktail (pubblico)
    [HttpGet]
    [Authorize(Policy = "AdminOrService")]
    public async Task<ActionResult<List<Cocktail>>> GetAll()
    {
        var cocktails = await _db.Cocktails
            .OrderBy(x => x.Name)
            .ToListAsync();

        return cocktails;
    }

    // ✅ CREA cocktail (solo Admin) → POST /cocktail/single
    [Authorize(Roles = "Admin")]
    [HttpPost("single")]
    public async Task<IActionResult> Create([FromBody] ApprovedCocktailDto dto)
    {
        var userId = User.FindFirst("sub")?.Value ?? "Unknown";
        var id = await _cocktailManager.CreateCocktailAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // ✅ GET cocktail by ID (JWT valido richiesto) → GET /cocktail/single/{id}
    [Authorize]
    [HttpGet("single/{id}")]
    public async Task<ActionResult<Cocktail>> GetById(Guid id)
    {
        var cocktail = await _cocktailManager.GetCocktailByIdAsync(id);
        if (cocktail == null)
            return NotFound();

        return Ok(cocktail);
    }

    // ✅ UPDATE cocktail (solo Admin) → PUT /cocktail/single/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("single/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ApprovedCocktailDto dto)
    {
        var updated = await _cocktailManager.UpdateCocktailAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    // ✅ DELETE cocktail (solo Admin) → DELETE /cocktail/single/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("single/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _cocktailManager.DeleteCocktailAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
