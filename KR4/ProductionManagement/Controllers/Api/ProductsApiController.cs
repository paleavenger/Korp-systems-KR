using KR5.Data;
using KR5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category = null)
    {
        var q = _db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            q = q.Where(p => p.Category == category);
        return Ok(await q.ToListAsync());
    }

    [HttpGet("{id}/materials")]
    public async Task<IActionResult> GetMaterials(int id)
    {
        var items = await _db.ProductMaterials
            .Where(pm => pm.ProductId == id)
            .Include(pm => pm.Material)
            .Select(pm => new { pm.Material!.Name, pm.Material.UnitOfMeasure, pm.QuantityNeeded })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.name))
            return BadRequest("Название обязательно");
        if (string.IsNullOrWhiteSpace(dto.category))
            return BadRequest("Категория обязательна");
        if (dto.prod_time <= 0)
            return BadRequest("Время производства должно быть больше 0");

        var product = new Product
        {
            Name = dto.name.Trim(),
            Description = dto.description?.Trim() ?? string.Empty,
            Category = dto.category.Trim(),
            ProductionTimePerUnit = dto.prod_time,
            MinimalStock = dto.minimalStock
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        if (dto.materials != null)
        {
            foreach (var m in dto.materials)
            {
                var mat = await _db.Materials.FindAsync(m.materialId);
                if (mat == null) continue;
                if (m.quantityNeeded <= 0) continue;
                _db.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = product.Id,
                    MaterialId = m.materialId,
                    QuantityNeeded = m.quantityNeeded
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.name))
            return BadRequest("Название обязательно");
        if (string.IsNullOrWhiteSpace(dto.category))
            return BadRequest("Категория обязательна");
        if (dto.prod_time <= 0)
            return BadRequest("Время производства должно быть больше 0");

        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound("Продукт не найден");

        product.Name = dto.name.Trim();
        product.Description = dto.description?.Trim() ?? string.Empty;
        product.Category = dto.category.Trim();
        product.ProductionTimePerUnit = dto.prod_time;
        product.MinimalStock = dto.minimalStock;

        if (dto.materials != null)
        {
            var existing = _db.ProductMaterials.Where(pm => pm.ProductId == id);
            _db.ProductMaterials.RemoveRange(existing);
            foreach (var m in dto.materials)
            {
                if (m.quantityNeeded <= 0) continue;
                var mat = await _db.Materials.FindAsync(m.materialId);
                if (mat == null) continue;
                _db.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = id,
                    MaterialId = m.materialId,
                    QuantityNeeded = m.quantityNeeded
                });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(product);
    }

    public record MaterialLink(int materialId, decimal quantityNeeded);
    public record CreateProductDto(string name, int prod_time, string category,
        string? description, int minimalStock, List<MaterialLink>? materials);
}
