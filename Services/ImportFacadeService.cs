using CocktailService.Importers;
using CocktailService.Services;

namespace CocktailService.Services;

public class ImportFacadeService
{
    private readonly CocktailImportSyncService _syncService;
    private readonly SearchSyncClient          _search;

    public ImportFacadeService(CocktailImportSyncService syncService,
                               SearchSyncClient          search)
    {
        _syncService = syncService;
        _search      = search;
    }

    public async Task RunFullImportAsync()
    {
        await _syncService.ImportAsync();         // gestisce i tre passi
        await _search.TriggerReloadAsync();       // notifica SearchService
    }
}
