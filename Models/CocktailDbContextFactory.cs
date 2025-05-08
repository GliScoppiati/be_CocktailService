using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocktailService.Models
{
    public class CocktailDbContextFactory : IDesignTimeDbContextFactory<CocktailDbContext>
    {
        public CocktailDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CocktailDbContext>();

            // Inserisci qui la connection string per le migration
            optionsBuilder.UseNpgsql("Host=localhost;Database=cocktail_db;Username=postgres;Password=postgres");

            return new CocktailDbContext(optionsBuilder.Options);
        }
    }
}
