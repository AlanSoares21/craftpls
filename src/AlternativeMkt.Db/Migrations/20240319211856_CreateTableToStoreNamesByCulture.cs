using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableToStoreNamesByCulture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "craft_items_data_by_culture",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Culture = table.Column<string>(type: "text", nullable: false),
                    itemid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("craft_items_data_by_culture_pkey", x => x.id);
                    table.ForeignKey(
                        name: "craft_items_data_by_culture_fkey",
                        column: x => x.itemid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_data_by_culture_itemid",
                table: "craft_items_data_by_culture",
                column: "itemid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "craft_items_data_by_culture");
        }
    }
}
