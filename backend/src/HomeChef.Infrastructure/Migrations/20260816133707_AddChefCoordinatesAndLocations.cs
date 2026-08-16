using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChefCoordinatesAndLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "homechef",
                table: "ChefProfiles",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "homechef",
                table: "ChefProfiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "homechef",
                table: "ChefProfiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_Area",
                schema: "homechef",
                table: "ChefProfiles",
                column: "Area");

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_City",
                schema: "homechef",
                table: "ChefProfiles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_City_Area",
                schema: "homechef",
                table: "ChefProfiles",
                columns: new[] { "City", "Area" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_Latitude_Longitude",
                schema: "homechef",
                table: "ChefProfiles",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_Area",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_City",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_City_Area",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_Latitude_Longitude",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "homechef",
                table: "ChefProfiles");
        }
    }
}
