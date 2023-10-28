using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddServerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_manufacturers_userid",
                table: "manufacturers");

            migrationBuilder.AddColumn<byte>(
                name: "serverid",
                table: "manufacturers",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "serverid",
                table: "game_accounts",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    id = table.Column<byte>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("server_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_serverid",
                table: "manufacturers",
                column: "serverid");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_userid",
                table: "manufacturers",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_game_accounts_serverid",
                table: "game_accounts",
                column: "serverid");

            migrationBuilder.AddForeignKey(
                name: "game_accounts_serverid_fkey",
                table: "game_accounts",
                column: "serverid",
                principalTable: "servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "manufacturers_serverid_fkey",
                table: "manufacturers",
                column: "serverid",
                principalTable: "servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "game_accounts_serverid_fkey",
                table: "game_accounts");

            migrationBuilder.DropForeignKey(
                name: "manufacturers_serverid_fkey",
                table: "manufacturers");

            migrationBuilder.DropTable(
                name: "servers");

            migrationBuilder.DropIndex(
                name: "IX_manufacturers_serverid",
                table: "manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_manufacturers_userid",
                table: "manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_game_accounts_serverid",
                table: "game_accounts");

            migrationBuilder.DropColumn(
                name: "serverid",
                table: "manufacturers");

            migrationBuilder.DropColumn(
                name: "serverid",
                table: "game_accounts");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_userid",
                table: "manufacturers",
                column: "userid",
                unique: true);
        }
    }
}
