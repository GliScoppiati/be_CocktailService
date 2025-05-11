using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocktailService.Services;

namespace CocktailService.Controllers;

[ApiController]
[Route("cocktail/import")]
public class ImportAllController : ControllerBase
{
    private readonly ImportFacadeService _facade;

    public ImportAllController(ImportFacadeService facade) => _facade = facade;

    [Authorize(Roles = "Admin")]
    [HttpPost("all")]
    public async Task<IActionResult> ImportAll()
    {
        await _facade.RunFullImportAsync();
        return Ok(new
        {
            message = "Full import completed.",
            importedAt = DateTime.UtcNow
        });
    }
}
