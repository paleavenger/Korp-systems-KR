namespace KR4.Models;

public class Attraction
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? WorkingHours { get; set; }
    public decimal? VisitCost { get; set; }

    public int CityId { get; set; }
    public City City { get; set; } = null!;
}
