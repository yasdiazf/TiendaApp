using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApp.Models;

public class Venta
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public decimal Monto { get; set; }

    public int UsuarioId { get; set; } // ID del usuario autenticado

    // Campos solo para la vista (No requieren cambios en PostgreSQL)
    [NotMapped]
    public string MetodoPago { get; set; } = "Efectivo";

    [NotMapped]
    public decimal MontoRecibido { get; set; }

    [NotMapped]
    public decimal Cambio { get; set; }
    public string? Observacion { get; set; }
}