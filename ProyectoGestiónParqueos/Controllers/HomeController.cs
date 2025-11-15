using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGestiónParqueos.Models;
using System.Diagnostics;

namespace ProyectoGestiónParqueos.Controllers
{
    public class HomeController : Controller
    {
        private readonly GestionParqueosDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(GestionParqueosDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Index con filtros de zonas y estados
        public IActionResult Index(string[]? zonas, string[]? estados)
        {
            // Base de la consulta
            var query = _context.Parqueo
                .Include(p => p.Estado)
                .Include(p => p.Zona)
                .AsQueryable();

            // ---- Filtros de zona ----
            if (zonas != null && zonas.Length > 0 && !zonas.Contains("TODOS"))
            {
                query = query.Where(p => zonas.Contains(p.Zona.nombre_zona));
            }

            // ---- Filtros de estado ----
            if (estados != null && estados.Length > 0)
            {
                query = query.Where(p => estados.Contains(p.Estado.nombreEstado));
            }

            // Traemos la lista filtrada
            var parqueosFiltrados = query
                .OrderBy(p => p.Zona.nombre_zona)
                .ThenBy(p => p.id_parqueo)
                .ToList();

            // Resumen (sobre el resultado filtrado)
            ViewBag.TotalLibre = parqueosFiltrados.Count(p =>
                p.Estado.nombreEstado.Equals("Libre", StringComparison.OrdinalIgnoreCase));

            ViewBag.TotalOcupado = parqueosFiltrados.Count(p =>
                p.Estado.nombreEstado.Equals("Ocupado", StringComparison.OrdinalIgnoreCase));

            ViewBag.TotalFuera = parqueosFiltrados.Count(p =>
                p.Estado.nombreEstado.Contains("fuera", StringComparison.OrdinalIgnoreCase));

            ViewBag.TotalTodos = parqueosFiltrados.Count;

            // Datos para los filtros
            ViewBag.Zonas = _context.ZonaParqueo
                .Select(z => z.nombre_zona)
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            ViewBag.ZonasSeleccionadas = zonas ?? Array.Empty<string>();
            ViewBag.EstadosSeleccionados = estados ?? Array.Empty<string>();

            return View(parqueosFiltrados);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
