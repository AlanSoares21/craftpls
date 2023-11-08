using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assetid",
                table: "craft_items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    endpoint = table.Column<string>(type: "character varying(69)", maxLength: 69, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("asset_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_assetid",
                table: "craft_items",
                column: "assetid");

            migrationBuilder.AddForeignKey(
                name: "craft_items_asset_fkey",
                table: "craft_items",
                column: "assetid",
                principalTable: "assets",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "craft_items_asset_fkey",
                table: "craft_items");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropIndex(
                name: "IX_craft_items_assetid",
                table: "craft_items");

            migrationBuilder.DropColumn(
                name: "assetid",
                table: "craft_items");
        }
    }
}
