using KR4.Models;
using Microsoft.EntityFrameworkCore;

namespace KR4.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<City> Cities { get; set; }
    public DbSet<Attraction> Attractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>().HasMany(c => c.Attractions)
            .WithOne(a => a.City)
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<City>().HasData(
            new City
            {
                Id = 1, Name = "Москва", Region = "Центральный федеральный округ",
                Population = 12506468,
                History = "Москва — столица России, один из крупнейших городов мира. Первое упоминание о Москве относится к 1147 году. Город был основан князем Юрием Долгоруким и с тех пор прошёл долгий путь от небольшого поселения до мегаполиса мирового значения.",
                CoatOfArmsUrl = "/images/Coat_of_arms_of_Moscow.svg.png",
                PhotoUrl = "/images/Moscow_dash_pic.jpg"
            },
            new City
            {
                Id = 2, Name = "Санкт-Петербург", Region = "Северо-Западный федеральный округ",
                Population = 5601911,
                History = "Санкт-Петербург основан Петром I в 1703 году и был столицей Российской империи более 200 лет. Город знаменит своей уникальной архитектурой, многочисленными музеями и культурными традициями. Его нередко называют «Северной Венецией» из-за множества рек и каналов.",
                CoatOfArmsUrl = "/images/Coat_of_arms_of_Saint_Petersburg_(2003).svg.png",
                PhotoUrl = "/images/SPB_dash.jpg"
            },
            new City
            {
                Id = 3, Name = "Казань", Region = "Приволжский федеральный округ",
                Population = 1257391,
                History = "Казань — столица Республики Татарстан, один из крупнейших городов России. Город был основан около 1000 лет назад и является крупным культурным, научным и спортивным центром страны. В 2013 году здесь прошла Универсиада, а в 2018 — матчи Чемпионата мира по футболу.",
                CoatOfArmsUrl = "/images/Coat_of_Arms_of_Kazan_(Tatarstan).svg.png",
                PhotoUrl = "/images/Kazan_dash.jpg"
            }
        );

        modelBuilder.Entity<Attraction>().HasData(
            new Attraction
            {
                Id = 1, CityId = 1, Name = "Красная площадь",
                Description = "Главная площадь России, сердце Москвы",
                History = "Красная площадь существует с конца XV века. На ней расположены Кремль, Собор Василия Блаженного, Мавзолей Ленина и ГУМ. Площадь является символом России и внесена в список Всемирного наследия ЮНЕСКО.",
                PhotoUrl = "/images/Moscow_red_square_attarction.jpg",
                WorkingHours = "Круглосуточно",
                VisitCost = null
            },
            new Attraction
            {
                Id = 2, CityId = 1, Name = "Храм Христа Спасителя",
                Description = "Главный кафедральный собор Русской православной церкви",
                History = "Храм Христа Спасителя был возведён в XIX веке в память о победе в Отечественной войне 1812 года. В советское время взорван, а в 1990-х годах воссоздан заново. Является одним из символов Москвы и крупнейшим православным храмом России.",
                PhotoUrl = "/images/Moscow_church_attarction.jpg",
                WorkingHours = "Пн–Вс: 10:00–17:00",
                VisitCost = null
            },
            new Attraction
            {
                Id = 3, CityId = 2, Name = "Эрмитаж",
                Description = "Один из крупнейших и наиболее значимых художественных и культурно-исторических музеев мира",
                History = "Музей основан в 1764 году Екатериной Великой. Коллекция насчитывает около 3 миллионов экспонатов. Главный корпус — Зимний дворец — был официальной резиденцией российских императоров.",
                PhotoUrl = "/images/Ermitage_attraction.jpg",
                WorkingHours = "Вт–Вс: 11:00–19:00, Ср: 11:00–21:00",
                VisitCost = 600
            },
            new Attraction
            {
                Id = 4, CityId = 2, Name = "Петергоф",
                Description = "Дворцово-парковый ансамбль, «Русский Версаль»",
                History = "Петергоф основан Петром I в начале XVIII века. Знаменит своей уникальной системой фонтанов, которая работает без насосов — только за счёт естественного перепада высот. Внесён в список Всемирного наследия ЮНЕСКО.",
                PhotoUrl = "/images/Peterhof_attraction.jpg",
                WorkingHours = "Пн–Вс: 09:00–20:00 (сезонно)",
                VisitCost = 1000
            },
            new Attraction
            {
                Id = 5, CityId = 3, Name = "Казанский Кремль",
                Description = "Уникальный памятник истории и архитектуры, объект Всемирного наследия ЮНЕСКО",
                History = "Кремль является древнейшей частью Казани. На его территории расположены мечеть Кул-Шариф, Благовещенский собор и другие исторические постройки. Включён в список Всемирного наследия ЮНЕСКО в 2000 году.",
                PhotoUrl = "/images/Kazan_attraction.jpg",
                WorkingHours = "Пн–Вс: 09:00–18:00",
                VisitCost = 300
            }
        );
    }
}
