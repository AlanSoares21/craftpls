using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class AlterEquipsTableToResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "equips_pkey",
                table: "equips");

            migrationBuilder.RenameTable(
                name: "equips",
                newName: "craft_resource");

            migrationBuilder.RenameIndex(
                name: "IX_equips_resourceid",
                table: "craft_resource",
                newName: "IX_craft_resource_resourceid");

            migrationBuilder.RenameIndex(
                name: "IX_equips_itemid",
                table: "craft_resource",
                newName: "IX_craft_resource_itemid");

            migrationBuilder.AddPrimaryKey(
                name: "craft_resource_pkey",
                table: "craft_resource",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "craft_resource_pkey",
                table: "craft_resource");

            migrationBuilder.RenameTable(
                name: "craft_resource",
                newName: "equips");

            migrationBuilder.RenameIndex(
                name: "IX_craft_resource_resourceid",
                table: "equips",
                newName: "IX_equips_resourceid");

            migrationBuilder.RenameIndex(
                name: "IX_craft_resource_itemid",
                table: "equips",
                newName: "IX_equips_itemid");

            migrationBuilder.AddPrimaryKey(
                name: "equips_pkey",
                table: "equips",
                column: "id");
        }
    }
}
