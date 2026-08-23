using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_ChefProfileId_IsAvailable",
                schema: "homechef",
                table: "FoodItems",
                columns: new[] { "ChefProfileId", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefMessages_ChefProfileId_ReadAtUtc",
                schema: "homechef",
                table: "ChefMessages",
                columns: new[] { "ChefProfileId", "ReadAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoodItems_ChefProfileId_IsAvailable",
                schema: "homechef",
                table: "FoodItems");

            migrationBuilder.DropIndex(
                name: "IX_ChefMessages_ChefProfileId_ReadAtUtc",
                schema: "homechef",
                table: "ChefMessages");
        }
    }
}
