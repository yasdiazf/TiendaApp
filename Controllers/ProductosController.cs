using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers;

// Clases DTO para recibir el carrito JSON de la vista de Ofertas
public class ItemOfertaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
}

public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==============================================================
    // RUTA PERSONALIZADA SEO-FRIENDLY (Requisito Actividad 3)
    // ==============================================================
    [Route("promociones-del-mes/barrio-norte")]
    public async Task<IActionResult> OfertasEspeciales()
    {
        var fechaLimite = DateTime.UtcNow.AddDays(60);

        var productosOferta = await _context.Productos
            .AsNoTracking()
            .Where(p => p.FechaVencimiento <= fechaLimite && p.Stock > 0)
            .OrderBy(p => p.FechaVencimiento)
            .Take(5)
            .ToListAsync();

        ViewData["Message"] = "Ofertas exclusivas en productos con vencimiento próximo. ¡Aprovecha antes de que se agoten!";
        return View(productosOferta);
    }

    // GET: /Productos
    public async Task<IActionResult> Index()
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        if (rol != "Almacen")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "Solo el usuario encargado de Artículos/Almacén puede ingresar a esta sección.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        var productos = await _context.Productos.AsNoTracking().ToListAsync();
        return View(productos);
    }

    // GET: /Productos/Create
    public IActionResult Create()
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        if (rol != "Almacen")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "No tienes permisos para registrar productos.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        return View();
    }

    // POST: /Productos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (ModelState.IsValid)
        {
            producto.FechaVencimiento = DateTime.SpecifyKind(producto.FechaVencimiento, DateTimeKind.Utc);

            _context.Add(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(producto);
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        if (rol != "Almacen")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "No tienes permisos para editar productos.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        if (id == null) return NotFound();

        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        return View(producto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                producto.FechaVencimiento = DateTime.SpecifyKind(producto.FechaVencimiento, DateTimeKind.Utc);

                _context.Update(producto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(producto);
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        if (rol != "Almacen")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "No tienes permisos para eliminar productos.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        if (id == null) return NotFound();

        var producto = await _context.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (producto == null) return NotFound();

        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ==============================================================
    // PROCESAR COMPRA DESDE OFERTAS ESPECIALES
    // ==============================================================
    [HttpPost]
    public async Task<IActionResult> ProcesarVentaOferta([FromBody] List<ItemOfertaDTO> items)
    {
        if (items == null || !items.Any())
        {
            return BadRequest(new { success = false, message = "El carrito está vacío." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal montoTotalVenta = 0;
            List<string> detallesCompra = new List<string>();

            foreach (var item in items)
            {
                var prodBD = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == item.Nombre);

                if (prodBD != null)
                {
                    if (prodBD.Stock < item.Cantidad)
                    {
                        return BadRequest(new { success = false, message = $"Stock insuficiente para {item.Nombre}" });
                    }

                    prodBD.Stock -= item.Cantidad;
                    montoTotalVenta += item.Precio * item.Cantidad;
                    detallesCompra.Add($"{item.Cantidad}x {item.Nombre}");
                }
            }

            int? usuarioIdSesion = HttpContext.Session.GetInt32("UsuarioId");
            int idUsuarioFinal = (usuarioIdSesion.HasValue && usuarioIdSesion.Value > 0) 
                ? usuarioIdSesion.Value 
                : 102; 

            var nuevaVenta = new Venta
            {
                Fecha = DateTime.UtcNow,
                Monto = montoTotalVenta,
                UsuarioId = idUsuarioFinal,
                Observacion = $"🔥 Compra del área Ofertas Especiales (-20% Desc.): {string.Join(", ", detallesCompra)}"
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "¡Venta de oferta registrada exitosamente!" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            string mensajeDetallado = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return StatusCode(500, new { success = false, message = "Error PostgreSQL: " + mensajeDetallado });
        }
    }

    private bool ProductoExists(int id)
    {
        return _context.Productos.Any(e => e.Id == id);
    }
}