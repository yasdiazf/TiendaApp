using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;
using TiendaApp.Models;

namespace TiendaApp.Controllers;

public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    // INYECCIÓN DE DEPENDENCIAS (DIP de SOLID)
    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Productos
    public async Task<IActionResult> Index()
    {
        // LINQ con AsNoTracking para optimizar lectura
        var productos = await _context.Productos.AsNoTracking().ToListAsync();
        return View(productos);
    }

    // GET: /Productos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Productos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (ModelState.IsValid)
        {
            // Convertir fecha a UTC para compatibilidad con PostgreSQL
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
                // Convertir fecha a UTC para compatibilidad con PostgreSQL
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

    private bool ProductoExists(int id)
    {
        return _context.Productos.Any(e => e.Id == id);
    }
}