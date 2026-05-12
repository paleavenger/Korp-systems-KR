using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KR4.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImagesAndChurch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoUrl",
                value: "/images/Moscow_red_square_attarction.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "History", "Name", "PhotoUrl", "VisitCost", "WorkingHours" },
                values: new object[] { "Главный кафедральный собор Русской православной церкви", "Храм Христа Спасителя был возведён в XIX веке в память о победе в Отечественной войне 1812 года. В советское время взорван, а в 1990-х годах воссоздан заново. Является одним из символов Москвы и крупнейшим православным храмом России.", "Храм Христа Спасителя", "/images/Moscow_church_attarction.jpg", null, "Пн–Вс: 10:00–17:00" });

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoUrl",
                value: "/images/Ermitage_attraction.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 4,
                column: "PhotoUrl",
                value: "/images/Peterhof_attraction.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 5,
                column: "PhotoUrl",
                value: "/images/Kazan_attraction.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "/images/Coat_of_arms_of_Moscow.svg.png", "/images/Moscow_dash_pic.jpg" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "/images/Coat_of_arms_of_Saint_Petersburg_(2003).svg.png", "/images/SPB_dash.jpg" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "/images/Coat_of_Arms_of_Kazan_(Tatarstan).svg.png", "/images/Kazan_dash.jpg" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhotoUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Moscow_Red_Square.jpg/1280px-Moscow_Red_Square.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "History", "Name", "PhotoUrl", "VisitCost", "WorkingHours" },
                values: new object[] { "Крупнейший музей русского изобразительного искусства", "Галерея основана в 1856 году купцом Павлом Третьяковым. В коллекции хранится более 180 000 произведений искусства — живопись, графика, скульптура, декоративно-прикладное искусство с XI по XX век.", "Третьяковская галерея", "https://upload.wikimedia.org/wikipedia/commons/thumb/5/52/Tretyakov_gallery.JPG/1280px-Tretyakov_gallery.JPG", 500m, "Вт–Вс: 10:00–18:00, Чт–Пт: 10:00–21:00" });

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 3,
                column: "PhotoUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b2/Hermitage_museum_2007.jpg/1280px-Hermitage_museum_2007.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 4,
                column: "PhotoUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bf/Grand_Cascade_Peterhof.jpg/1280px-Grand_Cascade_Peterhof.jpg");

            migrationBuilder.UpdateData(
                table: "Attractions",
                keyColumn: "Id",
                keyValue: 5,
                column: "PhotoUrl",
                value: "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4f/Kazan_Kremlin_towers.jpg/1280px-Kazan_Kremlin_towers.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/Coat_of_Arms_of_Moscow.svg/240px-Coat_of_Arms_of_Moscow.svg.png", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8c/Moscow_July_2011-7a.jpg/1280px-Moscow_July_2011-7a.jpg" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/thumb/7/77/Coat_of_Arms_of_Saint_Petersburg_%282003%29.svg/240px-Coat_of_Arms_of_Saint_Petersburg_%282003%29.svg.png", "https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Palace_Bridge_SPB.jpg/1280px-Palace_Bridge_SPB.jpg" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CoatOfArmsUrl", "PhotoUrl" },
                values: new object[] { "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4d/Coat_of_arms_of_Kazan.svg/240px-Coat_of_arms_of_Kazan.svg.png", "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f9/Kazan_Kremlin_from_across_Kazanka_river.jpg/1280px-Kazan_Kremlin_from_across_Kazanka_river.jpg" });
        }
    }
}
