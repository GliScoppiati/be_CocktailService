using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CocktailService.Models
{
    public class CocktailDbContextFactory : IDesignTimeDbContextFactory<CocktailDbContext>
    {
        public CocktailDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<CocktailDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CocktailDbContext(optionsBuilder.Options);
        }
    }
}