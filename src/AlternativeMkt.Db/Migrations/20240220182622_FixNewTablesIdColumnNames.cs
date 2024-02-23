using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class FixNewTablesIdColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "resources_provided",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "craft_items_attributes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "attributes",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "resources_provided",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "craft_items_attributes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "attributes",
                newName: "Id");
        }
    }
}
