using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeChef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodCategories",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "PKR"),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreparationTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodItems_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodItems_FoodCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "homechef",
                        principalTable: "FoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "homechef",
                table: "FoodCategories",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "DisplayOrder", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hearty, home-style traditional and continental main meals.", 1, "Main Course", "main-course" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aromatic spiced rice, authentic biryanis, and pulao specialties.", 2, "Rice & Biryani", "rice-biryani" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rich desi gravies, fresh wok karahis, and slow-cooked curries.", 3, "Karahi & Curries", "karahi-curries" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Charcoal grilled kebabs, tikkas, and smoked specialties.", 4, "BBQ & Grills", "bbq-grills" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Freshly baked customized cakes, artisanal breads, pastries, and cookies.", 5, "Bakery & Cakes", "bakery-cakes" },
                    { new Guid("11111111-1111-1111-1111-111111111106"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decadent puddings, traditional mithai, brownies, and treats.", 6, "Desserts & Sweets", "desserts-sweets" },
                    { new Guid("11111111-1111-1111-1111-111111111107"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Crispy samosas, rolls, chaat, and quick savory bites.", 7, "Snacks & Appetizers", "snacks-appetizers" },
                    { new Guid("11111111-1111-1111-1111-111111111108"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Morning favorites, stuffed parathas, halwa puri, and omelettes.", 8, "Breakfast & Parathas", "breakfast-parathas" },
                    { new Guid("11111111-1111-1111-1111-111111111109"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Homemade drinks, lassi, fresh juices, and specialty teas.", 9, "Beverages", "beverages" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodCategories_Slug",
                schema: "homechef",
                table: "FoodCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_CategoryId",
                schema: "homechef",
                table: "FoodItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_ChefProfileId",
                schema: "homechef",
                table: "FoodItems",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_CreatedAtUtc",
                schema: "homechef",
                table: "FoodItems",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_IsAvailable",
                schema: "homechef",
                table: "FoodItems",
                column: "IsAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodItems",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "FoodCategories",
                schema: "homechef");
        }
    }
}
