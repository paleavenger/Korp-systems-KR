using KR5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers;

public class ProductionLinesController : Controller
{
    private readonly AppDbContext _db;
    public ProductionLinesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var lines = await _db.ProductionLines.ToListAsync();
        ViewBag.Products = await _db.Products.ToListAsync();

        // Текущие заказы для каждой линии
        var activeOrders = await _db.WorkOrders
            .Include(w => w.Product)
            .Where(w => w.Status == "InProgress")
            .ToListAsync();
        ViewBag.ActiveOrders = activeOrders;

        return View(lines);
    }
}
