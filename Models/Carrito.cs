public class Carrito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    public List<CarritoItem> Items { get; set; } = new();
}

public class CarritoItem
{
    public int Id { get; set; }

    public int CarritoId { get; set; }
    public Carrito Carrito { get; set; } = null!;

    public string ProductoId { get; set; } = null!;
    public Producto Producto { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public class CarritoSesionItem
{
    public string ProductoId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}