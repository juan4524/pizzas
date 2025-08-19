using Microsoft.EntityFrameworkCore;

public class PizzeriaContext : DbContext
{
    public PizzeriaContext(DbContextOptions<PizzeriaContext> options) : base(options) {}

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItems => Set<PedidoItem>();

protected override void OnModelCreating(ModelBuilder mb)
{
    base.OnModelCreating(mb);

    // Usuario
    mb.Entity<Usuario>()
      .HasIndex(u => u.Correo)
      .IsUnique();

    mb.Entity<Usuario>()
      .HasOne(u => u.Carrito)
      .WithOne(c => c.Usuario)
      .HasForeignKey<Carrito>(c => c.UsuarioId);

    // Precisión de dinero (decimal 10,2)
    mb.Entity<Producto>()
      .Property(p => p.Precio)
      .HasPrecision(10, 2);

    mb.Entity<CarritoItem>()
      .Property(ci => ci.PrecioUnitario)
      .HasPrecision(10, 2);

    mb.Entity<CarritoItem>()
      .Property(ci => ci.Subtotal)
      .HasPrecision(10, 2);

    mb.Entity<Pedido>()
      .Property(p => p.Total)
      .HasPrecision(10, 2);

    mb.Entity<PedidoItem>()
      .Property(pi => pi.PrecioUnitario)
      .HasPrecision(10, 2);

    mb.Entity<PedidoItem>()
      .Property(pi => pi.Subtotal)
      .HasPrecision(10, 2);

    // CarritoItem → Producto (evitar borrado en cascada)
    mb.Entity<CarritoItem>()
      .HasOne(ci => ci.Producto)
      .WithMany()
      .HasForeignKey(ci => ci.ProductoId)
      .OnDelete(DeleteBehavior.Restrict);
    }

}