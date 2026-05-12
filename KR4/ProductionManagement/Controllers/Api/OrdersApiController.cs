using KR5.Data;
using KR5.Hubs;
using KR5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers.Api;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ProductionHub> _hub;

    public OrdersApiController(AppDbContext db, IHubContext<ProductionHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status = null, [FromQuery] string? date = null)
    {
        var q = _db.WorkOrders.Include(w => w.Product).Include(w => w.ProductionLine).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "active")
            q = q.Where(w => w.Status == status);
        if (status == "active")
            q = q.Where(w => w.Status == "InProgress" || w.Status == "Pending");
        if (date == "today")
            q = q.Where(w => w.StartDate.Date == DateTime.UtcNow.Date);
        return Ok(await q.ToListAsync());
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .Include(w => w.ProductionLine)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        if (dto.product_id <= 0)
            return BadRequest("Выберите продукт");
        if (dto.quantity <= 0)
            return BadRequest("Количество должно быть больше 0");

        var product = await _db.Products.Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(p => p.Id == dto.product_id);
        if (product == null) return NotFound("Продукт не найден");

        // Проверка материалов
        foreach (var pm in product.ProductMaterials)
        {
            var needed = pm.QuantityNeeded * dto.quantity;
            if (pm.Material!.Quantity < needed)
                return BadRequest($"Недостаточно материала «{pm.Material.Name}»: нужно {needed}, на складе {pm.Material.Quantity}");
        }

        ProductionLine? line = null;
        if (dto.line_id.HasValue)
        {
            line = await _db.ProductionLines.FindAsync(dto.line_id.Value);
            if (line == null) return NotFound("Линия не найдена");
            if (line.Status == "Stopped")
                return BadRequest($"Линия «{line.Name}» остановлена");
            }

        var startDate = dto.start_date.HasValue ? dto.start_date.Value.ToUniversalTime() : DateTime.UtcNow;
        var minutes = (double)(product.ProductionTimePerUnit * dto.quantity) / (line?.EfficiencyFactor ?? 1.0f);
        var endDate = startDate.AddMinutes(minutes);

        if (dto.line_id.HasValue)
        {
            var activeOrder = await _db.WorkOrders.FirstOrDefaultAsync(w =>
                w.ProductionLineId == dto.line_id && w.Status == "InProgress");
            if (activeOrder != null && startDate < activeOrder.EstimatedEndDate)
                return BadRequest($"На линии выполняется заказ #{activeOrder.Id}. Новый заказ можно начать не раньше {activeOrder.EstimatedEndDate.AddHours(3):dd/MM/yy HH:mm} МСК");

            var overlap = await _db.WorkOrders.AnyAsync(w =>
                w.ProductionLineId == dto.line_id &&
                w.Status != "Cancelled" && w.Status != "Completed" &&
                w.StartDate < endDate && w.EstimatedEndDate > startDate);
            if (overlap)
                return BadRequest("Выбранный промежуток времени пересекается с другим заказом на этой линии");
        }

        var order = new WorkOrder
        {
            ProductId = dto.product_id,
            ProductionLineId = dto.line_id,
            Quantity = dto.quantity,
            StartDate = startDate,
            EstimatedEndDate = endDate,
            Status = "Pending"
        };
        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPut("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressDto dto)
    {
        var order = await _db.WorkOrders.FindAsync(id);
        if (order == null) return NotFound();

        order.Progress = Math.Clamp(dto.percent, 0, 100);
        if (order.Progress == 100) order.Status = "Completed";
        await _db.SaveChangesAsync();

        // Уведомляем всех клиентов через SignalR
        await _hub.Clients.All.SendAsync("ReceiveProgress", id, order.Progress, order.Status);
        return Ok(new { order.Id, order.Progress, order.Status });
    }

    [HttpPut("{id}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        if (order.Status != "Pending") return BadRequest("Заказ не в статусе Pending");
        if (!order.ProductionLineId.HasValue)
            return BadRequest("Нельзя запустить заказ без назначенной производственной линии");

        if (order.ProductionLineId.HasValue)
        {
            var lineBusy = await _db.WorkOrders.AnyAsync(w =>
                w.ProductionLineId == order.ProductionLineId && w.Status == "InProgress" && w.Id != id);
            if (lineBusy)
                return BadRequest("Линия уже занята другим активным заказом");
        }

        // Списываем материалы
        foreach (var pm in order.Product.ProductMaterials)
        {
            var needed = pm.QuantityNeeded * order.Quantity;
            if (pm.Material!.Quantity < needed)
                return BadRequest($"Недостаточно материала «{pm.Material.Name}»");
            pm.Material.Quantity -= needed;
        }

        order.Status = "InProgress";
        order.MaterialsConsumed = true;
        order.StartDate = DateTime.UtcNow;

        var prodLine = await _db.ProductionLines.FindAsync(order.ProductionLineId.Value);
        var mins = (double)(order.Product.ProductionTimePerUnit * order.Quantity) / (prodLine?.EfficiencyFactor ?? 1.0f);
        order.EstimatedEndDate = order.StartDate.AddMinutes(mins);

        if (prodLine != null)
        {
            prodLine.Status = "Active";
            prodLine.CurrentWorkOrderId = order.Id;
        }

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ReceiveProgress", id, 0, "InProgress");
        return Ok(order);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        if (order.Status == "Completed" || order.Status == "Cancelled")
            return BadRequest("Нельзя отменить завершённый или уже отменённый заказ");

        // Возвращаем материалы если были списаны
        if (order.MaterialsConsumed)
        {
            foreach (var pm in order.Product.ProductMaterials)
                pm.Material!.Quantity += pm.QuantityNeeded * order.Quantity;
            order.MaterialsConsumed = false;
        }

        order.Status = "Cancelled";
        order.Progress = 0;

        if (order.ProductionLineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null && line.CurrentWorkOrderId == order.Id)
                line.CurrentWorkOrderId = null;
        }

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ReceiveProgress", id, 0, "Cancelled");
        return Ok(order);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDto dto)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();

        var newStart = dto.new_start.ToUniversalTime();
        if (newStart < DateTime.UtcNow)
            return BadRequest("Нельзя назначить время в прошлом");

        var line = order.ProductionLineId.HasValue
            ? await _db.ProductionLines.FindAsync(order.ProductionLineId.Value) : null;

        // InProgress → откат в Pending, возврат материалов
        if (order.Status == "InProgress")
        {
            if (order.MaterialsConsumed)
            {
                foreach (var pm in order.Product.ProductMaterials)
                    pm.Material!.Quantity += pm.QuantityNeeded * order.Quantity;
                order.MaterialsConsumed = false;
            }
            order.Status = "Pending";
            order.Progress = 0;
            if (line != null && line.CurrentWorkOrderId == order.Id)
                line.CurrentWorkOrderId = null;
            await _hub.Clients.All.SendAsync("ReceiveProgress", id, 0, "Pending");
        }

        order.StartDate = newStart;
        var minutes = (double)(order.Product.ProductionTimePerUnit * order.Quantity) / (line?.EfficiencyFactor ?? 1.0f);
        order.EstimatedEndDate = order.StartDate.AddMinutes(minutes);
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    public record CreateOrderDto(int product_id, int quantity, int? line_id, DateTime? start_date);
    public record UpdateProgressDto(int percent);
    public record RescheduleDto(DateTime new_start);
}
