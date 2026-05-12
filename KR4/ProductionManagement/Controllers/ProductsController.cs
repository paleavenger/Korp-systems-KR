using KR5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index([FromQuery] string? category, [FromQuery] string? search)
    {
        var q = _db.Products.Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material).AsQueryable();
        if (!string.IsNullOrEmpty(category)) q = q.Where(p => p.Category == category);
        if (!string.IsNullOrEmpty(search)) q = q.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        ViewBag.Categories = await _db.Products.Select(p => p.Category).Distinct().ToListAsync();
        ViewBag.Category = category;
        ViewBag.Search = search;
        ViewBag.Materials = await _db.Materials.ToListAsync();
        return View(await q.ToListAsync());
    }
}
