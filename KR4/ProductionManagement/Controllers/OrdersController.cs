using KR5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR5.Controllers;

public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index([FromQuery] string? status)
    {
        var q = _db.WorkOrders.Include(w => w.Product).Include(w => w.ProductionLine).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(w => w.Status == status);
        q = q.OrderByDescending(w => w.Id);

        ViewBag.Products = await _db.Products.ToListAsync();
        ViewBag.Lines = await _db.ProductionLines.Where(l => l.Status == "Active").ToListAsync();
        ViewBag.Status = status;
        return View(await q.ToListAsync());
    }
}
