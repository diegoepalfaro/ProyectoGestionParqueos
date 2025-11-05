using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGestiónParqueos.Models;

namespace ProyectoGestiónParqueos.Controllers
{
    public class ReportesController : Controller
    {
        private readonly GestionParqueosDbContext _context;

        public ReportesController(GestionParqueosDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reportes = _context.ReporteParqueo
                .Include(r => r.AsignacionParqueo)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.fechaHoraR)
                .ToList();

            return View("ReportesParqueo", reportes);
        }

        [HttpGet]
        public IActionResult CrearReporte()
        {
            return View();
        }
    }
}
