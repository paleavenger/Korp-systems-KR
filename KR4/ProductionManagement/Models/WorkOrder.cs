namespace KR5.Models;

public class WorkOrder
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? ProductionLineId { get; set; }
    public ProductionLine? ProductionLine { get; set; }

    public int Quantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EstimatedEndDate { get; set; }

    // Pending / InProgress / Completed / Cancelled
    public string Status { get; set; } = "Pending";

    public int Progress { get; set; } = 0;

    // true после того как материалы списаны со склада
    public bool MaterialsConsumed { get; set; } = false;
}
