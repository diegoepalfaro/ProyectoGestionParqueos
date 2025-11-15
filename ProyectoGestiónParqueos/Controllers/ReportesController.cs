using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                .ToList();

            return View("ReportesParqueo", reportes);
        }

        private List<SelectListItem> CargarAsignaciones()
        {
            return _context.AsignacionParqueo
                .Include(a => a.Parqueo).ThenInclude(p => p.Zona)
                .Include(a => a.Vehiculo)
                .Select(a => new SelectListItem
                {
                    Value = a.id_asignacionP.ToString(),
                    Text = $"Zona: {a.Parqueo!.Zona!.nombre_zona ?? "Sin zona"} - Vehículo: {a.Vehiculo!.placasV ?? "Sin placas"}"
                })
                .ToList();
        }

        [HttpGet]
        public IActionResult CrearReporte()
        {
            ViewBag.Asignaciones = CargarAsignaciones();
            return View();
        }

        [HttpPost]
        public IActionResult CrearReporte(reporteParqueo reporte)
        {
            ModelState.Remove("Usuario");
            ModelState.Remove("AsignacionParqueo");

            if (reporte.id_asignacionP == 0)
                ModelState.AddModelError(nameof(reporte.id_asignacionP), "Debe seleccionar una asignación de parqueo");

            if (string.IsNullOrWhiteSpace(reporte.estadoR))
                ModelState.AddModelError(nameof(reporte.estadoR), "Debe seleccionar un estado");

            if (string.IsNullOrWhiteSpace(reporte.descripcionR))
                ModelState.AddModelError(nameof(reporte.descripcionR), "La descripción es requerida");

            if (!ModelState.IsValid)
            {
                ViewBag.Asignaciones = CargarAsignaciones();
                return View(reporte);
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId is null)
                return RedirectToAction("Login", "Administrador");

            var nuevoReporte = new reporteParqueo
            {
            
                id_asignacionP = reporte.id_asignacionP,
                id_usuario = usuarioId.Value,
                estadoR = reporte.estadoR,
                descripcionR = reporte.descripcionR,
                fechaHoraR = DateTime.Now
            };

            _context.ReporteParqueo.Add(nuevoReporte);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}