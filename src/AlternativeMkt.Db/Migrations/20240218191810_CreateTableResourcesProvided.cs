using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableResourcesProvided : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resources_provided",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    requestid = table.Column<Guid>(type: "uuid", nullable: false),
                    resourceid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("resource_provided_key", x => x.Id);
                    table.ForeignKey(
                        name: "resources_provided_requestid_fkey",
                        column: x => x.requestid,
                        principalTable: "requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "resources_provided_resourceid_fkey",
                        column: x => x.resourceid,
                        principalTable: "craft_resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_resources_provided_requestid_resourceid",
                table: "resources_provided",
                columns: new[] { "requestid", "resourceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resources_provided_resourceid",
                table: "resources_provided",
                column: "resourceid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resources_provided");
        }
    }
}
