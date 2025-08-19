using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Utils;

public class ProductosController : Controller
{
    private readonly PizzeriaContext _db;
    public ProductosController(PizzeriaContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var productos = await _db.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Tipo).ThenBy(p => p.Nombre)
            .ToListAsync();
        return View(productos);
    }
}