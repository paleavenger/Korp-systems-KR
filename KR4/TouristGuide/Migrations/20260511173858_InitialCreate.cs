using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KR4.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Region = table.Column<string>(type: "text", nullable: false),
                    Population = table.Column<int>(type: "integer", nullable: false),
                    History = table.Column<string>(type: "text", nullable: false),
                    CoatOfArmsUrl = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    History = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    WorkingHours = table.Column<string>(type: "text", nullable: true),
                    VisitCost = table.Column<decimal>(type: "numeric", nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attractions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CoatOfArmsUrl", "History", "Name", "PhotoUrl", "Population", "Region" },
                values: new object[,]
                {
                    { 1, "https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/Coat_of_Arms_of_Moscow.svg/240px-Coat_of_Arms_of_Moscow.svg.png", "Москва — столица России, один из крупнейших городов мира. Первое упоминание о Москве относится к 1147 году. Город был основан князем Юрием Долгоруким и с тех пор прошёл долгий путь от небольшого поселения до мегаполиса мирового значения.", "Москва", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8c/Moscow_July_2011-7a.jpg/1280px-Moscow_July_2011-7a.jpg", 12506468, "Центральный федеральный округ" },
                    { 2, "https://upload.wikimedia.org/wikipedia/commons/thumb/7/77/Coat_of_Arms_of_Saint_Petersburg_%282003%29.svg/240px-Coat_of_Arms_of_Saint_Petersburg_%282003%29.svg.png", "Санкт-Петербург основан Петром I в 1703 году и был столицей Российской империи более 200 лет. Город знаменит своей уникальной архитектурой, многочисленными музеями и культурными традициями. Его нередко называют «Северной Венецией» из-за множества рек и каналов.", "Санкт-Петербург", "https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Palace_Bridge_SPB.jpg/1280px-Palace_Bridge_SPB.jpg", 5601911, "Северо-Западный федеральный округ" },
                    { 3, "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4d/Coat_of_arms_of_Kazan.svg/240px-Coat_of_arms_of_Kazan.svg.png", "Казань — столица Республики Татарстан, один из крупнейших городов России. Город был основан около 1000 лет назад и является крупным культурным, научным и спортивным центром страны. В 2013 году здесь прошла Универсиада, а в 2018 — матчи Чемпионата мира по футболу.", "Казань", "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f9/Kazan_Kremlin_from_across_Kazanka_river.jpg/1280px-Kazan_Kremlin_from_across_Kazanka_river.jpg", 1257391, "Приволжский федеральный округ" }
                });

            migrationBuilder.InsertData(
                table: "Attractions",
                columns: new[] { "Id", "CityId", "Description", "History", "Name", "PhotoUrl", "VisitCost", "WorkingHours" },
                values: new object[,]
                {
                    { 1, 1, "Главная площадь России, сердце Москвы", "Красная площадь существует с конца XV века. На ней расположены Кремль, Собор Василия Блаженного, Мавзолей Ленина и ГУМ. Площадь является символом России и внесена в список Всемирного наследия ЮНЕСКО.", "Красная площадь", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Moscow_Red_Square.jpg/1280px-Moscow_Red_Square.jpg", null, "Круглосуточно" },
                    { 2, 1, "Крупнейший музей русского изобразительного искусства", "Галерея основана в 1856 году купцом Павлом Третьяковым. В коллекции хранится более 180 000 произведений искусства — живопись, графика, скульптура, декоративно-прикладное искусство с XI по XX век.", "Третьяковская галерея", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/52/Tretyakov_gallery.JPG/1280px-Tretyakov_gallery.JPG", 500m, "Вт–Вс: 10:00–18:00, Чт–Пт: 10:00–21:00" },
                    { 3, 2, "Один из крупнейших и наиболее значимых художественных и культурно-исторических музеев мира", "Музей основан в 1764 году Екатериной Великой. Коллекция насчитывает около 3 миллионов экспонатов. Главный корпус — Зимний дворец — был официальной резиденцией российских императоров.", "Эрмитаж", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b2/Hermitage_museum_2007.jpg/1280px-Hermitage_museum_2007.jpg", 600m, "Вт–Вс: 11:00–19:00, Ср: 11:00–21:00" },
                    { 4, 2, "Дворцово-парковый ансамбль, «Русский Версаль»", "Петергоф основан Петром I в начале XVIII века. Знаменит своей уникальной системой фонтанов, которая работает без насосов — только за счёт естественного перепада высот. Внесён в список Всемирного наследия ЮНЕСКО.", "Петергоф", "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bf/Grand_Cascade_Peterhof.jpg/1280px-Grand_Cascade_Peterhof.jpg", 1000m, "Пн–Вс: 09:00–20:00 (сезонно)" },
                    { 5, 3, "Уникальный памятник истории и архитектуры, объект Всемирного наследия ЮНЕСКО", "Кремль является древнейшей частью Казани. На его территории расположены мечеть Кул-Шариф, Благовещенский собор и другие исторические постройки. Включён в список Всемирного наследия ЮНЕСКО в 2000 году.", "Казанский Кремль", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4f/Kazan_Kremlin_towers.jpg/1280px-Kazan_Kremlin_towers.jpg", 300m, "Пн–Вс: 09:00–18:00" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attractions_CityId",
                table: "Attractions",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attractions");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
