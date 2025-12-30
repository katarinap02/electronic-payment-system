using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebShopAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebShopAdmins",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WebShopId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebShopAdmins", x => new { x.UserId, x.WebShopId });
                    table.ForeignKey(
                        name: "FK_WebShopAdmins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebShopAdmins_WebShops_WebShopId",
                        column: x => x.WebShopId,
                        principalTable: "WebShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebShopAdmins_WebShopId",
                table: "WebShopAdmins",
                column: "WebShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebShopAdmins");
        }
    }
}
