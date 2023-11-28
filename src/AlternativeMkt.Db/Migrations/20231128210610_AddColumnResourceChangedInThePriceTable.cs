using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnResourceChangedInThePriceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "resourceschanged",
                table: "craft_items_prices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "resourceschanged",
                table: "craft_items_prices");
        }
    }
}
