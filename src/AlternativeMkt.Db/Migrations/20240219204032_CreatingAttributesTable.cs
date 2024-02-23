using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreatingAttributesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("attributes_key", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "craft_items_attributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemid = table.Column<int>(type: "integer", nullable: false),
                    attributeid = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("craft_items_attributes_key", x => x.Id);
                    table.ForeignKey(
                        name: "craft_items_attributes_attributeid_fkey",
                        column: x => x.attributeid,
                        principalTable: "attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "craft_items_attributes_itemid_fkey",
                        column: x => x.itemid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_attributes_attributeid",
                table: "craft_items_attributes",
                column: "attributeid");

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_attributes_itemid",
                table: "craft_items_attributes",
                column: "itemid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "craft_items_attributes");

            migrationBuilder.DropTable(
                name: "attributes");
        }
    }
}
