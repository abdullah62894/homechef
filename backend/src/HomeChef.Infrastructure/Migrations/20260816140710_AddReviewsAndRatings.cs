using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ChefProfileId",
                schema: "homechef",
                table: "Reviews",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ChefProfileId_CustomerUserId",
                schema: "homechef",
                table: "Reviews",
                columns: new[] { "ChefProfileId", "CustomerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CreatedAtUtc",
                schema: "homechef",
                table: "Reviews",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerUserId",
                schema: "homechef",
                table: "Reviews",
                column: "CustomerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "homechef");
        }
    }
}
