using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChefMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChefMessages",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefMessages_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChefMessages_ChefProfileId",
                schema: "homechef",
                table: "ChefMessages",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefMessages_ChefProfileId_CreatedAtUtc",
                schema: "homechef",
                table: "ChefMessages",
                columns: new[] { "ChefProfileId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefMessages_SenderUserId",
                schema: "homechef",
                table: "ChefMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefMessages_SenderUserId_CreatedAtUtc",
                schema: "homechef",
                table: "ChefMessages",
                columns: new[] { "SenderUserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChefMessages",
                schema: "homechef");
        }
    }
}
