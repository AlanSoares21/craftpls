using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNameCulture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Culture",
                table: "craft_items_data_by_culture",
                newName: "culture");

            migrationBuilder.AlterColumn<string>(
                name: "culture",
                table: "craft_items_data_by_culture",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "culture",
                table: "craft_items_data_by_culture",
                newName: "Culture");

            migrationBuilder.AlterColumn<string>(
                name: "Culture",
                table: "craft_items_data_by_culture",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
