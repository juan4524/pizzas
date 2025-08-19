using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pizzeria.Utils;
public class CarritoController : Controller
{
    private readonly PizzeriaContext _db;
    public CarritoController(PizzeriaContext db) => _db = db;

    private int? UID => HttpContext.Session.GetInt32(SesionKeys.UsuarioId);

    // GET /Carrito
    public async Task<IActionResult> Index()
    {
        if (UID is int uid)
        {
            var carrito = await _db.Carritos
                .Include(c => c.Items).ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == uid) 
                ?? new Carrito { UsuarioId = uid, Items = new() };

            ViewBag.Total = carrito.Items.Sum(i => i.Subtotal);
            return View(carrito);
        }
        else
        {
            var anon = HttpContext.Session.GetJson<List<CarritoSesionItem>>(SesionKeys.CarritoAnon) ?? new();
            ViewBag.Total = anon.Sum(i => i.Subtotal);
            // Reusar la vista con un Carrito "virtual"
            var carVirtual = new Carrito { Items = anon.Select(a => new CarritoItem {
                Id = 0, ProductoId = a.ProductoId,
                PrecioUnitario = a.PrecioUnitario, Cantidad = a.Cantidad, Subtotal = a.Subtotal,
                Producto = new Producto { Id = a.ProductoId, Nombre = a.Nombre, Tipo = a.Tipo, Precio = a.PrecioUnitario, Activo = true }
            }).ToList() };
            return View(carVirtual);
        }
    }

    // POST /Carrito/Agregar
    [HttpPost]
    public async Task<IActionResult> Agregar(string idProducto)
    {
        var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == idProducto && p.Activo);
        if (prod == null) return NotFound("Producto no encontrado");

        if (UID is int uid) // autenticado -> a BD
        {
            var carrito = await _db.Carritos
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UsuarioId == uid);

            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = uid, ActualizadoEn = DateTime.UtcNow, Items = new() };
                _db.Carritos.Add(carrito);
            }

            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == idProducto);
            if (item == null)
                carrito.Items.Add(new CarritoItem { ProductoId = prod.Id, PrecioUnitario = prod.Precio, Cantidad = 1, Subtotal = prod.Precio });
            else
            {
                item.Cantidad += 1;
                item.Subtotal = item.Cantidad * item.PrecioUnitario;
            }

            carrito.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        else // anónimo -> a sesión
        {
            var lista = HttpContext.Session.GetJson<List<CarritoSesionItem>>(SesionKeys.CarritoAnon) ?? new();
            var it = lista.FirstOrDefault(x => x.ProductoId == idProducto);
            if (it == null)
                lista.Add(new CarritoSesionItem { ProductoId = prod.Id, Nombre = prod.Nombre, Tipo = prod.Tipo, PrecioUnitario = prod.Precio, Cantidad = 1 });
            else
                it.Cantidad += 1;

            HttpContext.Session.SetJson(SesionKeys.CarritoAnon, lista);
        }

        TempData["ok"] = "Agregado al carrito";
        return RedirectToAction("Index", "Productos");
    }

    // POST /Carrito/CambiarCantidad
    [HttpPost]
    public async Task<IActionResult> CambiarCantidad(int itemId, int cantidad, string? productoId)
    {
        cantidad = Math.Max(1, cantidad);

        if (UID is int uid) // BD
        {
            var item = await _db.CarritoItems.Include(i => i.Carrito).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return NotFound();
            item.Cantidad = cantidad;
            item.Subtotal = item.Cantidad * item.PrecioUnitario;
            item.Carrito!.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        else // sesión (usa productoId)
        {
            var lista = HttpContext.Session.GetJson<List<CarritoSesionItem>>(SesionKeys.CarritoAnon) ?? new();
            var it = lista.FirstOrDefault(x => x.ProductoId == productoId);
            if (it == null) return RedirectToAction(nameof(Index));
            it.Cantidad = cantidad;
            HttpContext.Session.SetJson(SesionKeys.CarritoAnon, lista);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Carrito/Eliminar
    [HttpPost]
    public async Task<IActionResult> Eliminar(int itemId, string? productoId)
    {
        if (UID is int uid) // BD
        {
            var item = await _db.CarritoItems.Include(i => i.Carrito).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return NotFound();
            _db.CarritoItems.Remove(item);
            if (item.Carrito != null) item.Carrito.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        else // sesión
        {
            var lista = HttpContext.Session.GetJson<List<CarritoSesionItem>>(SesionKeys.CarritoAnon) ?? new();
            lista.RemoveAll(x => x.ProductoId == productoId);
            HttpContext.Session.SetJson(SesionKeys.CarritoAnon, lista);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Carrito/Comprar
    [HttpPost]
    public async Task<IActionResult> Comprar()
{
    var uid = HttpContext.Session.GetInt32(SesionKeys.UsuarioId);
    if (uid == null)
    {
        TempData["err"] = "Debes iniciar sesión para comprar.";
        return RedirectToAction("Login", "Usuarios");
    }

    var carrito = await _db.Carritos
        .Include(c => c.Items).ThenInclude(i => i.Producto)
        .FirstOrDefaultAsync(c => c.UsuarioId == uid);

    if (carrito == null || carrito.Items.Count == 0)
    {
        TempData["err"] = "Tu carrito está vacío.";
        return RedirectToAction("Index");
    }

    var pedido = new Pedido
    {
        UsuarioId = uid.Value,
        CreadoEn = DateTime.UtcNow,
        EstadoPedido = "creado",
        Total = carrito.Items.Sum(i => i.Subtotal),
        Items = carrito.Items.Select(i => new PedidoItem
        {
            ProductoId = i.ProductoId,
            NombreProducto = i.Producto!.Nombre,
            TipoProducto = i.Producto!.Tipo,
            PrecioUnitario = i.PrecioUnitario,
            Cantidad = i.Cantidad,
            Subtotal = i.Subtotal
        }).ToList()
    };

    _db.Pedidos.Add(pedido);
    _db.CarritoItems.RemoveRange(carrito.Items); // vaciar carrito
    await _db.SaveChangesAsync();

    TempData["ok"] = "Compra realizada con éxito.";
    return RedirectToAction("MisPedidos", "Pedidos");
  }
}