using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageThumbnails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageThumbnailUrl",
                schema: "homechef",
                table: "FoodItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoThumbnailUrl",
                schema: "homechef",
                table: "ChefProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageThumbnailUrl",
                schema: "homechef",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "PhotoThumbnailUrl",
                schema: "homechef",
                table: "ChefProfiles");
        }
    }
}
