using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteChefs",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteChefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteChefs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteChefs_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteFoods",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteFoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteFoods_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteFoods_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteChefs_ChefProfileId",
                schema: "homechef",
                table: "FavoriteChefs",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteChefs_UserId",
                schema: "homechef",
                table: "FavoriteChefs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteChefs_UserId_ChefProfileId",
                schema: "homechef",
                table: "FavoriteChefs",
                columns: new[] { "UserId", "ChefProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteFoods_FoodItemId",
                schema: "homechef",
                table: "FavoriteFoods",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteFoods_UserId",
                schema: "homechef",
                table: "FavoriteFoods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteFoods_UserId_FoodItemId",
                schema: "homechef",
                table: "FavoriteFoods",
                columns: new[] { "UserId", "FoodItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteChefs",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "FavoriteFoods",
                schema: "homechef");
        }
    }
}
