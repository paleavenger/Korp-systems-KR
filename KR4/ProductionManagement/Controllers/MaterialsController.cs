using KR5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers;

public class MaterialsController : Controller
{
    private readonly AppDbContext _db;
    public MaterialsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var materials = await _db.Materials.OrderBy(m => m.Name).ToListAsync();
        return View(materials);
    }
}
