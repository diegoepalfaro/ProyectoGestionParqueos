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

        public IActionResult Index(string[] zonas, string[] estados)
        {
            var parqueos = _context.Parqueo
                .Include(p => p.Estado)
                .Include(p => p.Zona)
                .AsQueryable();

            if (zonas != null && zonas.Length > 0 && !zonas.Contains("TODOS"))
            {
                parqueos = parqueos.Where(p => zonas.Contains(p.Zona.nombre_zona));
            }

            if (estados != null && estados.Length > 0)
            {
                parqueos = parqueos.Where(p => estados.Contains(p.Estado.nombreEstado));
            }

            ViewBag.ZonasSeleccionadas = zonas ?? new string[0];
            ViewBag.EstadosSeleccionados = estados ?? new string[0];
            ViewBag.Zonas = _context.ZonaParqueo
                .Select(z => z.nombre_zona)
                .Distinct()
                .ToList();

            return View(parqueos.ToList());
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
