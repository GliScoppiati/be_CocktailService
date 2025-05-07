using Microsoft.EntityFrameworkCore;

namespace CocktailService.Models;

public class CocktailDbContext : DbContext
{
    public CocktailDbContext(DbContextOptions<CocktailDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cocktail> Cocktails { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<CocktailIngredient> CocktailIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Unicità del nome normalizzato per evitare duplicati
        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.NormalizedName)
            .IsUnique();

        // ✅ Relazione Cocktail -> CocktailIngredient
        modelBuilder.Entity<CocktailIngredient>()
            .HasOne(ci => ci.Cocktail)
            .WithMany(c => c.CocktailIngredients) // <-- CORRETTO
            .HasForeignKey(ci => ci.CocktailId);

        // ✅ Relazione CocktailIngredient -> Ingredient
        modelBuilder.Entity<CocktailIngredient>()
            .HasOne(ci => ci.Ingredient)
            .WithMany() // Non serve referenza inversa
            .HasForeignKey(ci => ci.IngredientId);
    }
}
