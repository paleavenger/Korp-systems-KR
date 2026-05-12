using KR5.Data;
using KR5.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers.Api;

[ApiController]
[Route("api/lines")]
public class LinesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ProductionHub> _hub;

    public LinesApiController(AppDbContext db, IHubContext<ProductionHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool available = false)
    {
        var q = _db.ProductionLines.AsQueryable();
        if (available) q = q.Where(l => l.Status == "Active" && l.CurrentWorkOrderId == null);
        return Ok(await q.ToListAsync());
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto dto)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();

        if (dto.status == "Stopped")
        {
            var activeOrder = await _db.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(w => w.ProductionLineId == id && w.Status == "InProgress");

            if (activeOrder != null)
            {
                if (activeOrder.MaterialsConsumed)
                {
                    foreach (var pm in activeOrder.Product.ProductMaterials)
                        pm.Material!.Quantity += pm.QuantityNeeded * activeOrder.Quantity;
                    activeOrder.MaterialsConsumed = false;
                }
                activeOrder.Status = "Pending";
                activeOrder.Progress = 0;
                await _hub.Clients.All.SendAsync("ReceiveProgress", activeOrder.Id, 0, "Pending");
            }

            line.CurrentWorkOrderId = null;
        }

        line.Status = dto.status;
        await _db.SaveChangesAsync();
        return Ok(line);
    }

    [HttpPut("{id}/efficiency")]
    public async Task<IActionResult> SetEfficiency(int id, [FromBody] SetEfficiencyDto dto)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();
        line.EfficiencyFactor = Math.Clamp(dto.factor, 0.5f, 2.0f);

        if (line.CurrentWorkOrderId.HasValue)
        {
            var order = await _db.WorkOrders
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == line.CurrentWorkOrderId && w.Status == "InProgress");
            if (order != null)
            {
                var minutes = (double)(order.Product.ProductionTimePerUnit * order.Quantity) / line.EfficiencyFactor;
                order.EstimatedEndDate = order.StartDate.AddMinutes(minutes);
                await _hub.Clients.All.SendAsync("UpdateOrderEndDate", order.Id,
                    DateTime.SpecifyKind(order.EstimatedEndDate, DateTimeKind.Utc).ToString("o"));
            }
        }

        await _db.SaveChangesAsync();
        return Ok(line);
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var orders = await _db.WorkOrders
            .Where(w => w.ProductionLineId == id && w.Status != "Cancelled" && w.Status != "Completed")
            .Include(w => w.Product)
            .Select(w => new { w.Id, ProductName = w.Product.Name, w.Quantity, w.Status, w.StartDate, w.EstimatedEndDate, w.Progress })
            .ToListAsync();
        return Ok(orders);
    }

    public record SetStatusDto(string status);
    public record SetEfficiencyDto(float factor);
}
