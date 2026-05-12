namespace KR5.Models;

public class ProductionLine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Stopped";
    public float EfficiencyFactor { get; set; } = 1.0f;
    public int? CurrentWorkOrderId { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
