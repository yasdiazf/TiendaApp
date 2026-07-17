using Microsoft.AspNetCore.Mvc;

namespace TiendaApp.Controllers;

public class AccountController : Controller
{
    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string usuario, string password)
    {
        // 1. Trabajador de Almacén (ejemplo: Yasmin)
        if (usuario == "yasmin.diaz" && password == "123456")
        {
            HttpContext.Session.SetString("UsuarioRol", "Almacen");
            HttpContext.Session.SetInt32("UsuarioId", 101); // Su ID de trabajador

            return RedirectToAction("Index", "Productos");
        }
        // 2. Vendedor 1 (ejemplo: Julieth)
        else if (usuario == "julieth.flores" && password == "123456")
        {
            HttpContext.Session.SetString("UsuarioRol", "Vendedor");
            HttpContext.Session.SetInt32("UsuarioId", 102); // Su ID de trabajador

            return RedirectToAction("Index", "Ventas");
        }
        // 3. Vendedor 2 (ejemplo: Nelson)
        else if (usuario == "nelson.huanca" && password == "123456")
        {
            HttpContext.Session.SetString("UsuarioRol", "Vendedor");
            HttpContext.Session.SetInt32("UsuarioId", 103); // Su ID único de trabajador

            return RedirectToAction("Index", "Ventas");
        }

        ViewBag.Error = "Usuario o contraseña incorrectos.";
        return View();
    }

    // GET: /Account/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}