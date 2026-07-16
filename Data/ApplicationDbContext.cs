using Microsoft.EntityFrameworkCore;
using TiendaApp.Models;

namespace TiendaApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<Producto> Productos { get; set; } = null!;
}