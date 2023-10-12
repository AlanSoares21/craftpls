using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "craft_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("craft_categories_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "craft_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    categoryid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("craft_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "craft_items_craftcategoryid_fkey",
                        column: x => x.categoryid,
                        principalTable: "craft_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "game_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("game_accounts_pkey", x => x.id);
                    table.ForeignKey(
                        name: "game_accounts_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "manufacturers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    maxrequestsopen = table.Column<short>(type: "smallint", nullable: false),
                    maxrequestsaccepted = table.Column<short>(type: "smallint", nullable: false),
                    hide = table.Column<bool>(type: "boolean", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("manufacturers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "manufacturers_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "equips",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ammount = table.Column<short>(type: "smallint", nullable: true),
                    resourceid = table.Column<int>(type: "integer", nullable: false),
                    itemid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("equips_pkey", x => x.id);
                    table.ForeignKey(
                        name: "craftresource_itemid_fkey",
                        column: x => x.itemid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "craftresource_resourceid_fkey",
                        column: x => x.resourceid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "craft_items_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    price = table.Column<int>(type: "integer", nullable: false),
                    itemid = table.Column<int>(type: "integer", nullable: false),
                    manufacturerid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("craft_items_prices_pkey", x => x.id);
                    table.ForeignKey(
                        name: "craft_items_prices_craftitemid_fkey",
                        column: x => x.itemid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "craft_items_prices_manufacturerid_fkey",
                        column: x => x.manufacturerid,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    price = table.Column<int>(type: "integer", nullable: false),
                    itemid = table.Column<int>(type: "integer", nullable: false),
                    requesterid = table.Column<Guid>(type: "uuid", nullable: false),
                    manufacturerid = table.Column<Guid>(type: "uuid", nullable: false),
                    cancelled = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    refused = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    accepted = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    finishedbymanufacturer = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    finishedbyrequester = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("requests_pkey", x => x.id);
                    table.ForeignKey(
                        name: "requests_itemid_fkey",
                        column: x => x.itemid,
                        principalTable: "craft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "requests_manufacturerid_fkey",
                        column: x => x.manufacturerid,
                        principalTable: "manufacturers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "requests_requesterid_fkey",
                        column: x => x.requesterid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "craft_items_name_key",
                table: "craft_items",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_categoryid",
                table: "craft_items",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_prices_itemid",
                table: "craft_items_prices",
                column: "itemid");

            migrationBuilder.CreateIndex(
                name: "IX_craft_items_prices_manufacturerid",
                table: "craft_items_prices",
                column: "manufacturerid");

            migrationBuilder.CreateIndex(
                name: "IX_equips_itemid",
                table: "equips",
                column: "itemid");

            migrationBuilder.CreateIndex(
                name: "IX_equips_resourceid",
                table: "equips",
                column: "resourceid");

            migrationBuilder.CreateIndex(
                name: "IX_game_accounts_userid",
                table: "game_accounts",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_manufacturers_userid",
                table: "manufacturers",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_requests_itemid",
                table: "requests",
                column: "itemid");

            migrationBuilder.CreateIndex(
                name: "IX_requests_manufacturerid",
                table: "requests",
                column: "manufacturerid");

            migrationBuilder.CreateIndex(
                name: "IX_requests_requesterid",
                table: "requests",
                column: "requesterid");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "craft_items_prices");

            migrationBuilder.DropTable(
                name: "equips");

            migrationBuilder.DropTable(
                name: "game_accounts");

            migrationBuilder.DropTable(
                name: "requests");

            migrationBuilder.DropTable(
                name: "craft_items");

            migrationBuilder.DropTable(
                name: "manufacturers");

            migrationBuilder.DropTable(
                name: "craft_categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
