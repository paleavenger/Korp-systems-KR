using KR4.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KR4.Controllers;

public class CitiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Cities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

        ViewBag.Search = search;
        var cities = await query.OrderBy(c => c.Name).ToListAsync();
        return View(cities);
    }

    public async Task<IActionResult> Details(int id)
    {
        var city = await _context.Cities
            .Include(c => c.Attractions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (city == null) return NotFound();
        return View(city);
    }
}
