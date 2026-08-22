using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentReports",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetFoodItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentReports_AspNetUsers_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentReports_ChefProfiles_TargetChefProfileId",
                        column: x => x.TargetChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentReports_FoodItems_TargetFoodItemId",
                        column: x => x.TargetFoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentReports_Reviews_TargetReviewId",
                        column: x => x.TargetReviewId,
                        principalSchema: "homechef",
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_CreatedAtUtc",
                schema: "homechef",
                table: "ContentReports",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ReporterUserId",
                schema: "homechef",
                table: "ContentReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ReporterUserId_Status",
                schema: "homechef",
                table: "ContentReports",
                columns: new[] { "ReporterUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_Status",
                schema: "homechef",
                table: "ContentReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_TargetChefProfileId",
                schema: "homechef",
                table: "ContentReports",
                column: "TargetChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_TargetFoodItemId",
                schema: "homechef",
                table: "ContentReports",
                column: "TargetFoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_TargetReviewId",
                schema: "homechef",
                table: "ContentReports",
                column: "TargetReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentReports",
                schema: "homechef");
        }
    }
}
