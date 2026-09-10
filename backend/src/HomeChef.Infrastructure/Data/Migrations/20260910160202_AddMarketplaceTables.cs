using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeChef.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalAtUtc",
                schema: "homechef",
                table: "ChefProfiles",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                schema: "homechef",
                table: "ChefProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryRadiusKm",
                schema: "homechef",
                table: "ChefProfiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "homechef",
                table: "ChefProfiles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "homechef",
                table: "ChefProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                schema: "homechef",
                table: "ChefProfiles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChefAvailabilities",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefAvailabilities_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefDeliveryMethods",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefDeliveryMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefDeliveryMethods_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "New"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cuisines",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealCategories",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChefProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "PKR"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DeliveryMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    WhatsAppInitiatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_CustomerUserId",
                        column: x => x.CustomerUserId,
                        principalSchema: "homechef",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalSchema: "homechef",
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodCuisines",
                schema: "homechef",
                columns: table => new
                {
                    FoodItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuisineId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodCuisines", x => new { x.FoodItemId, x.CuisineId });
                    table.ForeignKey(
                        name: "FK_FoodCuisines_Cuisines_CuisineId",
                        column: x => x.CuisineId,
                        principalSchema: "homechef",
                        principalTable: "Cuisines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodCuisines_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodAvailabilities",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableDays = table.Column<int[]>(type: "integer[]", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsAllDay = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MealCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodAvailabilities_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodAvailabilities_MealCategories_MealCategoryId",
                        column: x => x.MealCategoryId,
                        principalSchema: "homechef",
                        principalTable: "MealCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FoodMealCategories",
                schema: "homechef",
                columns: table => new
                {
                    FoodItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealCategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodMealCategories", x => new { x.FoodItemId, x.MealCategoryId });
                    table.ForeignKey(
                        name: "FK_FoodMealCategories_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodMealCategories_MealCategories_MealCategoryId",
                        column: x => x.MealCategoryId,
                        principalSchema: "homechef",
                        principalTable: "MealCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "homechef",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DishName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "PKR"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalSchema: "homechef",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "homechef",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "homechef",
                table: "Cuisines",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "DisplayOrder", "ImageThumbnailUrl", "ImageUrl", "IsActive", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Authentic Pakistani and traditional desi home-cooked meals.", 1, null, null, true, "Pakistani / Desi", "pakistani-desi" },
                    { new Guid("22222222-2222-2222-2222-222222222202"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aromatic layered rice dishes with premium spices and tender meat.", 2, null, null, true, "Biryani", "biryani" },
                    { new Guid("22222222-2222-2222-2222-222222222203"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Indo-Chinese and authentic Chinese stir-fry, noodles, and rice.", 3, null, null, true, "Chinese", "chinese" },
                    { new Guid("22222222-2222-2222-2222-222222222204"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Burgers, wraps, rolls, and quick bites made at home.", 4, null, null, true, "Fast Food", "fast-food" },
                    { new Guid("22222222-2222-2222-2222-222222222205"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Charcoal-grilled kebabs, tikkas, and smoked meat specialties.", 5, null, null, true, "BBQ & Grills", "bbq-grills" },
                    { new Guid("22222222-2222-2222-2222-222222222206"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Custom cakes, artisanal breads, pastries, and cookies.", 6, null, null, true, "Bakery & Cakes", "bakery-cakes" },
                    { new Guid("22222222-2222-2222-2222-222222222207"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decadent puddings, traditional mithai, brownies, and sweet treats.", 7, null, null, true, "Desserts & Sweets", "desserts-sweets" },
                    { new Guid("22222222-2222-2222-2222-222222222208"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Western-style pasta, steaks, salads, and continental dishes.", 8, null, null, true, "Continental", "continental" }
                });

            migrationBuilder.InsertData(
                schema: "homechef",
                table: "MealCategories",
                columns: new[] { "Id", "CreatedAtUtc", "DisplayOrder", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Breakfast", "breakfast" },
                    { new Guid("33333333-3333-3333-3333-333333333302"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Lunch", "lunch" },
                    { new Guid("33333333-3333-3333-3333-333333333303"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Dinner", "dinner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_ApprovalStatus",
                schema: "homechef",
                table: "ChefProfiles",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ChefAvailabilities_ChefProfileId",
                schema: "homechef",
                table: "ChefAvailabilities",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefAvailabilities_ChefProfileId_DayOfWeek",
                schema: "homechef",
                table: "ChefAvailabilities",
                columns: new[] { "ChefProfileId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefDeliveryMethods_ChefProfileId",
                schema: "homechef",
                table: "ChefDeliveryMethods",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefDeliveryMethods_ChefProfileId_Method",
                schema: "homechef",
                table: "ChefDeliveryMethods",
                columns: new[] { "ChefProfileId", "Method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedAtUtc",
                schema: "homechef",
                table: "ContactMessages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_IsRead",
                schema: "homechef",
                table: "ContactMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_Status",
                schema: "homechef",
                table: "ContactMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_DisplayOrder",
                schema: "homechef",
                table: "Cuisines",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_IsActive",
                schema: "homechef",
                table: "Cuisines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_Slug",
                schema: "homechef",
                table: "Cuisines",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodAvailabilities_FoodItemId",
                schema: "homechef",
                table: "FoodAvailabilities",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodAvailabilities_MealCategoryId",
                schema: "homechef",
                table: "FoodAvailabilities",
                column: "MealCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCuisines_CuisineId",
                schema: "homechef",
                table: "FoodCuisines",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodCuisines_FoodItemId",
                schema: "homechef",
                table: "FoodCuisines",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodMealCategories_FoodItemId",
                schema: "homechef",
                table: "FoodMealCategories",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodMealCategories_MealCategoryId",
                schema: "homechef",
                table: "FoodMealCategories",
                column: "MealCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MealCategories_Slug",
                schema: "homechef",
                table: "MealCategories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_FoodItemId",
                schema: "homechef",
                table: "OrderItems",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "homechef",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ChefProfileId",
                schema: "homechef",
                table: "Orders",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ChefProfileId_Status",
                schema: "homechef",
                table: "Orders",
                columns: new[] { "ChefProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAtUtc",
                schema: "homechef",
                table: "Orders",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerUserId",
                schema: "homechef",
                table: "Orders",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                schema: "homechef",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChefAvailabilities",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "ChefDeliveryMethods",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "ContactMessages",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "FoodAvailabilities",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "FoodCuisines",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "FoodMealCategories",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "Cuisines",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "MealCategories",
                schema: "homechef");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "homechef");

            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_ApprovalStatus",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "ApprovalAtUtc",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "DeliveryRadiusKm",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "homechef",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                schema: "homechef",
                table: "ChefProfiles");
        }
    }
}
