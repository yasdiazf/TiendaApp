using System.ComponentModel.DataAnnotations;

namespace TiendaApp.Models;

// RECOMENDACIÓN SOLID: Una clase debe tener una sola responsabilidad (SRP)
public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 10000, ErrorMessage = "El precio debe ser mayor a cero")]
    public decimal Precio { get; set; }

    public int Stock { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Vencimiento")]
    public DateTime FechaVencimiento { get; set; }
}