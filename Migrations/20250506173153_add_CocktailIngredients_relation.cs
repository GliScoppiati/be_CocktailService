using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailService.Migrations
{
    /// <inheritdoc />
    public partial class add_CocktailIngredients_relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Cocktails");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Cocktails",
                newName: "Instructions");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Cocktails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnit",
                table: "CocktailIngredients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityValue",
                table: "CocktailIngredients",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cocktails");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
                table: "CocktailIngredients");

            migrationBuilder.DropColumn(
                name: "QuantityValue",
                table: "CocktailIngredients");

            migrationBuilder.RenameColumn(
                name: "Instructions",
                table: "Cocktails",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Cocktails",
                type: "text",
                nullable: true);
        }
    }
}
