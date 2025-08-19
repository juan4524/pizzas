using System.ComponentModel.DataAnnotations;

public class Usuario
{
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Correo { get; set; } = null!;

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = null!;

    [Required]
    public string ContrasenaHash { get; set; } = null!;

    public string Estado { get; set; } = "activo"; // activo | eliminado
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public Carrito? Carrito { get; set; }
    public List<Pedido> Pedidos { get; set; } = new();
}