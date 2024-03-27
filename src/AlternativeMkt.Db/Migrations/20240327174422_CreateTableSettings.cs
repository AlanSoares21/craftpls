using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<byte>(type: "smallint", nullable: false),
                    aboutinstance = table.Column<string>(type: "text", nullable: false),
                    aboutinstanceshort = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    aboutcraftpls = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("settings_key", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "settings");
        }
    }
}
