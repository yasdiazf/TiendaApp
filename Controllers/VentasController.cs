using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers;

public class VentasController : Controller
{
    private readonly ApplicationDbContext _context;

    public VentasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Ventas
    public async Task<IActionResult> Index()
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        // Si el rol no es Vendedor (ej. Almacen), bloqueamos y destruimos la sesión
        if (rol != "Vendedor")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "Solo el personal del área de Ventas tiene acceso a este módulo.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        var ventas = await _context.Ventas.AsNoTracking().ToListAsync();
        return View(ventas);
    }

    // GET: /Ventas/Create
    public async Task<IActionResult> Create()
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");

        if (string.IsNullOrEmpty(rol))
        {
            return RedirectToAction("Login", "Account");
        }

        if (rol != "Vendedor")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "Solo el personal del área de Ventas puede registrar ventas.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        // Carga los productos de la BD para el Punto de Venta
        ViewBag.Productos = await _context.Productos.AsNoTracking().ToListAsync();

        return View();
    }

    // POST: /Ventas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Venta venta)
    {
        int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

        if (usuarioId.HasValue)
        {
            venta.UsuarioId = usuarioId.Value;
        }

        venta.Fecha = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            _context.Add(venta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Si falla la validación, recargamos la lista de productos
        ViewBag.Productos = await _context.Productos.AsNoTracking().ToListAsync();
        return View(venta);
    }

    // GET: /Ventas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");
        if (string.IsNullOrEmpty(rol)) return RedirectToAction("Login", "Account");

        if (rol != "Vendedor")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "No tienes permisos para editar registros de ventas.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        if (id == null) return NotFound();

        var venta = await _context.Ventas.FindAsync(id);
        if (venta == null) return NotFound();

        return View(venta);
    }

    // POST: /Ventas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Venta venta)
    {
        if (id != venta.Id) return NotFound();

        // Convertir el Kind de la fecha a UTC para PostgreSQL
        venta.Fecha = DateTime.SpecifyKind(venta.Fecha, DateTimeKind.Utc);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ventas.Any(e => e.Id == venta.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(venta);
    }

    // GET: /Ventas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        string? rol = HttpContext.Session.GetString("UsuarioRol");
        if (string.IsNullOrEmpty(rol)) return RedirectToAction("Login", "Account");

        if (rol != "Vendedor")
        {
            HttpContext.Session.Clear();
            ViewData["MensajeError"] = "No tienes permisos para eliminar ventas.";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        if (id == null) return NotFound();

        var venta = await _context.Ventas.FirstOrDefaultAsync(m => m.Id == id);
        if (venta == null) return NotFound();

        return View(venta);
    }

    // POST: /Ventas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta != null)
        {
            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}