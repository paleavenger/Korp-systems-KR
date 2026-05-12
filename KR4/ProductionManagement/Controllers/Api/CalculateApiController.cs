using KR5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers.Api;

[ApiController]
[Route("api/calculate")]
public class CalculateApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public CalculateApiController(AppDbContext db) => _db = db;

    [HttpPost("production")]
    public async Task<IActionResult> Calculate([FromBody] CalcDto dto)
    {
        var product = await _db.Products.FindAsync(dto.product_id);
        if (product == null) return NotFound();

        var lines = await _db.ProductionLines.Where(l => l.Status == "Active").ToListAsync();

        var results = lines.Select(l => new
        {
            LineId = l.Id,
            LineName = l.Name,
            EfficiencyFactor = l.EfficiencyFactor,
            TotalMinutes = (double)(product.ProductionTimePerUnit * dto.quantity) / l.EfficiencyFactor,
            EstimatedEnd = DateTime.UtcNow.AddMinutes((double)(product.ProductionTimePerUnit * dto.quantity) / l.EfficiencyFactor)
        }).ToList();

        var baseMinutes = product.ProductionTimePerUnit * dto.quantity;

        return Ok(new
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = dto.quantity,
            BaseMinutes = baseMinutes,
            ByLine = results
        });
    }

    public record CalcDto(int product_id, int quantity);
}
