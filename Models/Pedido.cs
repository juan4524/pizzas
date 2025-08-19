public class Pedido
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public string EstadoPedido { get; set; } = "creado"; // creado | pagado | entregado | cancelado
    public decimal Total { get; set; }

    public List<PedidoItem> Items { get; set; } = new();
}

public class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public string ProductoId { get; set; } = null!;
    public string NombreProducto { get; set; } = null!;
    public string TipoProducto { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}