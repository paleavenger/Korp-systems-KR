using KR5.Data;
using KR5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers.Api;

[ApiController]
[Route("api/materials")]
public class MaterialsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public MaterialsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool low_stock = false)
    {
        var q = _db.Materials.AsQueryable();
        if (low_stock) q = q.Where(m => m.Quantity <= m.MinimalStock);
        return Ok(await q.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.name))
            return BadRequest("Название обязательно");
        if (string.IsNullOrWhiteSpace(dto.unit))
            return BadRequest("Единица измерения обязательна");
        if (dto.quantity < 0)
            return BadRequest("Количество не может быть отрицательным");
        if (dto.min_stock < 0)
            return BadRequest("Минимальный запас не может быть отрицательным");

        var material = new Material
        {
            Name = dto.name.Trim(),
            Quantity = dto.quantity,
            UnitOfMeasure = dto.unit.Trim(),
            MinimalStock = dto.min_stock
        };
        _db.Materials.Add(material);
        await _db.SaveChangesAsync();
        return Ok(material);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        var material = await _db.Materials.FindAsync(id);
        if (material == null) return NotFound("Материал не найден");
        if (dto.amount == 0)
            return BadRequest("Укажите количество для пополнения");
        if (dto.amount < 0)
            return BadRequest("Количество для пополнения должно быть положительным");
        material.Quantity += dto.amount;
        await _db.SaveChangesAsync();
        return Ok(material);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.name))
            return BadRequest("Название обязательно");
        if (string.IsNullOrWhiteSpace(dto.unit))
            return BadRequest("Единица измерения обязательна");
        if (decimal.TryParse(dto.unit.Trim(), out _))
            return BadRequest("Единица измерения не может быть числом (используйте: кг, шт, литр...)");
        if (dto.quantity < 0)
            return BadRequest("Количество не может быть отрицательным");
        if (dto.min_stock < 0)
            return BadRequest("Минимальный запас не может быть отрицательным");

        var material = await _db.Materials.FindAsync(id);
        if (material == null) return NotFound("Материал не найден");

        material.Name = dto.name.Trim();
        material.UnitOfMeasure = dto.unit.Trim();
        material.Quantity = dto.quantity;
        material.MinimalStock = dto.min_stock;
        await _db.SaveChangesAsync();
        return Ok(material);
    }

    public record CreateMaterialDto(string name, decimal quantity, string unit, decimal min_stock);
    public record UpdateStockDto(decimal amount);
    public record UpdateMaterialDto(string name, decimal quantity, string unit, decimal min_stock);
}
