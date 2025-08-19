using System.ComponentModel.DataAnnotations;

public class Producto
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = null!;  // ID del producto, puede ser un GUID o un slug
    [Required, MaxLength(120)]
    public string Nombre { get; set; } = null!;

    [Required, MaxLength(16)]
    public string Tipo { get; set; } = null!; // pizza | bebida | snack | combo

    [Required]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;
}