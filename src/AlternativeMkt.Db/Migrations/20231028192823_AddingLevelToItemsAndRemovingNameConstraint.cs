using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddingLevelToItemsAndRemovingNameConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "craft_items_name_key",
                table: "craft_items");

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "craft_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "craft_items");

            migrationBuilder.CreateIndex(
                name: "craft_items_name_key",
                table: "craft_items",
                column: "name",
                unique: true);
        }
    }
}
