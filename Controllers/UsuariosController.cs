using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Pizzeria.Utils;

public class UsuariosController : Controller
{
    private readonly PizzeriaContext _db;
    public UsuariosController(PizzeriaContext db) => _db = db;

    // ---------- Registro ----------
    public IActionResult Registro() => View();

    [HttpPost]
    public async Task<IActionResult> Registro(string nombre, string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            TempData["err"] = "Completa todos los campos.";
            return View();
        }

        if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
        {
            TempData["err"] = "Ese correo ya está registrado.";
            return View();
        }

        var u = new Usuario {
            Nombre = nombre,
            Correo = correo,
            ContrasenaHash = Hash(contrasena),
            Estado = "activo"
        };
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync();

        // autenticamos al nuevo usuario (sesión)
        HttpContext.Session.SetInt32(SesionKeys.UsuarioId, u.Id);
        HttpContext.Session.SetString(SesionKeys.UsuarioNombre, u.Nombre);

        // migrar carrito anónimo (si existe) a BD
        await MigrarCarritoAnonimoABase(u.Id);

        TempData["ok"] = "Cuenta creada con éxito. Redirigiendo al inicio…";
        return RedirectToAction("Index", "Productos");
    }

    // ---------- Login ----------
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        var hash = Hash(contrasena);
        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo && x.ContrasenaHash == hash && x.Estado == "activo");
        if (u == null)
        {
            TempData["err"] = "Correo o contraseña incorrectos.";
            return View();
        }

        HttpContext.Session.SetInt32(SesionKeys.UsuarioId, u.Id);
        HttpContext.Session.SetString(SesionKeys.UsuarioNombre, u.Nombre);

        await MigrarCarritoAnonimoABase(u.Id);

        TempData["ok"] = $"Bienvenido, {u.Nombre}.";
        return RedirectToAction("Index", "Productos");
    }

    // ---------- Logout ----------
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["ok"] = "Sesión cerrada.";
        return RedirectToAction("Index", "Productos");
    }

    // ----- Helpers -----
    private static string Hash(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }

    [NonAction] // importante
    public async Task MigrarCarritoAnonimoABase(int usuarioId) // migrar carrito anónimo a BD
    {
        var anon = HttpContext.Session.GetJson<List<CarritoSesionItem>>(SesionKeys.CarritoAnon); // obtener carrito anónimo        
        if (anon == null || anon.Count == 0) return;

        var carrito = await _db.Carritos // buscar carrito del usuario
           .Include(c => c.Items)
           .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (carrito == null)
        {
            carrito = new Carrito { UsuarioId = usuarioId, ActualizadoEn = DateTime.UtcNow, Items = new() };
            _db.Carritos.Add(carrito);
        }

        foreach (var i in anon)
        {
            var existente = carrito.Items.FirstOrDefault(x => x.ProductoId == i.ProductoId);
            if (existente == null)
            {
                carrito.Items.Add(new CarritoItem {
                    ProductoId = i.ProductoId,
                    PrecioUnitario = i.PrecioUnitario,
                    Cantidad = i.Cantidad,
                    Subtotal = i.Cantidad * i.PrecioUnitario
                });
            }
            else
            {
                existente.Cantidad += i.Cantidad;
                existente.Subtotal = existente.Cantidad * existente.PrecioUnitario;
            }
        }

        carrito.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // limpiar carrito de sesión
        HttpContext.Session.Remove(SesionKeys.CarritoAnon);
    }
}