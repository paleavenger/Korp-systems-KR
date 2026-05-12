using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KR5.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    MinimalStock = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EfficiencyFactor = table.Column<float>(type: "real", nullable: false),
                    CurrentWorkOrderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Specifications = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    MinimalStock = table.Column<int>(type: "integer", nullable: false),
                    ProductionTimePerUnit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => new { x.ProductId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductionLineId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    MaterialsConsumed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "MinimalStock", "Name", "Quantity", "UnitOfMeasure" },
                values: new object[,]
                {
                    { 1, 100m, "Сталь", 500m, "кг" },
                    { 2, 50m, "Резина", 200m, "кг" },
                    { 3, 100m, "Пластик", 80m, "кг" },
                    { 4, 75m, "Алюминий", 350m, "кг" }
                });

            migrationBuilder.InsertData(
                table: "ProductionLines",
                columns: new[] { "Id", "CurrentWorkOrderId", "EfficiencyFactor", "Name", "Status" },
                values: new object[,]
                {
                    { 1, null, 1f, "Линия №1", "Active" },
                    { 2, null, 1.2f, "Линия №2", "Stopped" },
                    { 3, null, 0.8f, "Линия №3", "Active" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Description", "MinimalStock", "Name", "ProductionTimePerUnit", "Specifications" },
                values: new object[,]
                {
                    { 1, "Механика", "Стальная шестерня для редуктора", 50, "Шестерня А", 30, "{\"диаметр\":\"120мм\",\"зубья\":24}" },
                    { 2, "Корпуса", "Пластиковый корпус для электроники", 30, "Корпус Б", 20, "{\"размер\":\"200x150x80мм\",\"цвет\":\"черный\"}" },
                    { 3, "Уплотнители", "Резиновая прокладка уплотнительная", 200, "Прокладка В", 5, "{\"диаметр\":\"50мм\",\"толщина\":\"5мм\"}" }
                });

            migrationBuilder.InsertData(
                table: "ProductMaterials",
                columns: new[] { "MaterialId", "ProductId", "QuantityNeeded" },
                values: new object[,]
                {
                    { 1, 1, 2.5m },
                    { 4, 1, 0.5m },
                    { 3, 2, 0.8m },
                    { 2, 3, 0.2m }
                });

            migrationBuilder.InsertData(
                table: "WorkOrders",
                columns: new[] { "Id", "EstimatedEndDate", "MaterialsConsumed", "ProductId", "ProductionLineId", "Progress", "Quantity", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 12, 0, 58, 50, 702, DateTimeKind.Utc).AddTicks(8230), true, 1, 1, 45, 10, new DateTime(2026, 5, 10, 20, 58, 50, 702, DateTimeKind.Utc).AddTicks(8224), "InProgress" },
                    { 2, new DateTime(2026, 5, 12, 3, 58, 50, 702, DateTimeKind.Utc).AddTicks(8236), false, 2, null, 0, 20, new DateTime(2026, 5, 11, 20, 58, 50, 702, DateTimeKind.Utc).AddTicks(8235), "Pending" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_MaterialId",
                table: "ProductMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductId",
                table: "WorkOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionLineId",
                table: "WorkOrders",
                column: "ProductionLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMaterials");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ProductionLines");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
