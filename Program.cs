using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Data;

// Habilitar el comportamiento de timestamps previo de Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configuración de la Base de Datos (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Controladores y Vistas (MVC)
builder.Services.AddControllersWithViews();

// REGISTRO DE HTTPCONTEXTACCESSOR
builder.Services.AddHttpContextAccessor();

// CONFIGURACIÓN DEL SERVICIO DE SESIÓN
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// -------------------------------------------------------------
// CONFIGURACIÓN CULTURAL DE BOLIVIA (Formato de moneda Bs. y punto decimal)
// -------------------------------------------------------------
var cultureInfo = new CultureInfo("es-BO");
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencySymbol = "Bs. ";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configuración de entorno de producción / desarrollo
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ==============================================================
// 1. MIDDLEWARE DE MONITOREO DE RENDIMIENTO (Requisito Actividad 3)
// RECOMENDACIÓN SOLID: Cumple con el Principio de Responsabilidad Única (SRP).
// ODS 9 / Escalabilidad: Mide la latencia de cada solicitud HTTP sin mezclar 
// lógica en los controladores, permitiendo auditar cuellos de botella.
// ==============================================================
app.Use(async (context, next) =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();
    context.Response.Headers.Append("X-Server-Performance", "Tracking");

    await next();

    timer.Stop();
    Console.WriteLine($"[MONITOR] {context.Request.Method} {context.Request.Path} {timer.ElapsedMilliseconds}ms");
});

// ==============================================================
// 2. MIDDLEWARE DE MAPA DE SITIO (Sitemap XML)
// ODS 9 / Visibilidad: Genera de forma dinámica el mapa del sitio en XML
// sin requerir un archivo físico, permitiendo que motores como Google
// indexen los productos y ofertas de Doña Martha.
// ==============================================================
app.Map("/sitemap.xml", (appBuilder) =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/xml";
        var sitemapContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
    <url><loc>https://laeconomica.com/</loc><priority>1.0</priority></url>
    <url><loc>https://laeconomica.com/Productos</loc><priority>0.8</priority></url>
    <url><loc>https://laeconomica.com/Ofertas</loc><priority>0.9</priority></url>
</urlset>";

        await context.Response.WriteAsync(sitemapContent);
    });
});

app.UseStaticFiles();

app.UseRouting();

// 3. ACTIVAR MIDDLEWARE DE SESIÓN
app.UseSession();

app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();