using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Utils;
public class PedidosController : Controller
{
    private readonly PizzeriaContext _db;
    public PedidosController(PizzeriaContext db) => _db = db;

    public async Task<IActionResult> MisPedidos()
    {
        var uid = HttpContext.Session.GetInt32(SesionKeys.UsuarioId);
        if (uid == null)
        {
            TempData["err"] = "Debes iniciar sesión.";
            return RedirectToAction("Login", "Usuarios");
        }

        var pedidos = await _db.Pedidos
            .Include(p => p.Items)
            .Where(p => p.UsuarioId == uid)
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return View(pedidos);
    }
}