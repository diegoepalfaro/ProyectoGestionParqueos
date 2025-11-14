using Microsoft.AspNetCore.Mvc;
using ProyectoGestiónParqueos.Models;
using ZXing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;
using Microsoft.EntityFrameworkCore;

public class UsuarioController : Controller
{
    private readonly GestionParqueosDbContext _context;

    public UsuarioController(GestionParqueosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // ==========================================================
    //  ESCANEO GENERAL - ENTRADA / SALIDA AUTOMÁTICO
    // ==========================================================
    [HttpPost]
    public IActionResult LeerQR([FromBody] ImagenDto dto)
    {
        // 1. Decodificar QR
        byte[] bytes = Convert.FromBase64String(dto.ImagenBase64.Split(',')[1]);
        using var img = Image.Load<Rgba32>(bytes);
        var result = new BarcodeReaderGeneric().Decode(img);

        if (result == null)
            return Json(new { success = false });

        string carnet = result.Text;

        // 2. Buscar usuario y vehículo
        var usuario = _context.Usuario.FirstOrDefault(u => u.carnet == carnet);
        if (usuario == null)
            return Json(new { success = false, tipo = "NoRegistrado" });

        if (usuario.id_tipoUsuario == 1 || usuario.id_tipoUsuario == 2)
            return Json(new { success = false, tipo = "SinPermiso" });

        var vehiculo = _context.Vehiculo.FirstOrDefault(v => v.id_user == usuario.id_usuario);
        if (vehiculo == null)
            return Json(new { success = false, tipo = "NoTieneVehiculo" });


        // ==========================================================
        // 🔥 1. SALIDA PRIORITARIA
        // ==========================================================
        var asignacionesActivas = _context.AsignacionParqueo
            .Where(a => a.id_vehiculo == vehiculo.id_vehiculo && a.Hora_Salida == null)
            .ToList();

        if (asignacionesActivas.Count > 0)
        {
            foreach (var asig in asignacionesActivas)
            {
                // SOLO registramos salida — el trigger hace el resto
                _context.Database.ExecuteSqlRaw(@"
                    UPDATE asignacionParqueo
                    SET hora_salida = {0}
                    WHERE id_asignacionP = {1};
                ", DateTime.Now, asig.id_asignacionP);
            }

            return Json(new
            {
                success = true,
                tipo = "Salida",
                usuario = usuario.nombre,
                carnet = usuario.carnet,
                totalLiberados = asignacionesActivas.Count
            });
        }


        // ==========================================================
        // 🔥 2. ENTRADA (solo si NO tiene asignaciones activas)
        // ==========================================================
        int tipo = usuario.id_tipoUsuario;

        // ------------ DOCENTE (Zona D) ------------
        if (tipo == 3)
        {
            var parqueoLibre = _context.Parqueo
                .Where(p => p.id_zona == 4 && p.id_estadoP == 1)
                .OrderBy(p => p.id_parqueo)
                .FirstOrDefault();

            if (parqueoLibre == null)
                return Json(new { success = false, tipo = "NoDisponibleD" });

            _context.Database.ExecuteSqlRaw(@"
                INSERT INTO asignacionParqueo (id_vehiculo, id_parqueo, hora_entrada, hora_salida)
                VALUES ({0}, {1}, {2}, NULL);
            ", vehiculo.id_vehiculo, parqueoLibre.id_parqueo, DateTime.Now);

            parqueoLibre.id_estadoP = 2;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                tipo = "Docente",
                usuario = usuario.nombre,
                carnet = usuario.carnet,
                parqueo = parqueoLibre.id_parqueo,
                zona = "Zona D",
                hora = DateTime.Now
            });
        }

        // ------------ INVITADO (Zona C) ------------
        if (tipo == 5)
        {
            var parqueoLibre = _context.Parqueo
                .Where(p => p.id_zona == 3 && p.id_estadoP == 1)
                .OrderBy(p => p.id_parqueo)
                .FirstOrDefault();

            if (parqueoLibre == null)
                return Json(new { success = false, tipo = "NoDisponibleC" });

            _context.Database.ExecuteSqlRaw(@"
                INSERT INTO asignacionParqueo (id_vehiculo, id_parqueo, hora_entrada, hora_salida)
                VALUES ({0}, {1}, {2}, NULL);
            ", vehiculo.id_vehiculo, parqueoLibre.id_parqueo, DateTime.Now);

            parqueoLibre.id_estadoP = 2;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                tipo = "Invitado",
                usuario = usuario.nombre,
                carnet = usuario.carnet,
                parqueo = parqueoLibre.id_parqueo,
                zona = "Zona C",
                hora = DateTime.Now
            });
        }

        // ------------ ESTUDIANTE (elige zona A/B/C) ------------
        if (tipo == 4)
        {
            return Json(new
            {
                success = true,
                tipo = "Estudiante",
                usuario = new
                {
                    nombre = usuario.nombre,
                    carnet = usuario.carnet,
                    idTipoVehiculo = vehiculo.id_tipoV
                }
            });
        }

        return Json(new { success = false, tipo = "Desconocido" });
    }


    // ==========================================================
    //  ASIGNAR ZONA A ESTUDIANTE
    // ==========================================================
    [HttpPost]
    public IActionResult AsignarZonaEstudiante([FromBody] ZonaEstudianteDto data)
    {
        var usuario = _context.Usuario.First(u => u.carnet == data.carnet);
        var vehiculo = _context.Vehiculo.First(v => v.id_user == usuario.id_usuario);

        int zonaElegida = data.zona;
        int tipoVehiculo = vehiculo.id_tipoV;

        bool compatible = _context.Parqueo.Any(p =>
            p.id_zona == zonaElegida && p.id_tipoV == tipoVehiculo);

        // Regla: si no es compatible, siempre Zona B
        if (!compatible)
            zonaElegida = 2;

        var parqueoLibre = _context.Parqueo
            .Where(p => p.id_zona == zonaElegida && p.id_estadoP == 1)
            .OrderBy(p => p.id_parqueo)
            .FirstOrDefault();

        if (parqueoLibre == null)
            return Json(new { success = false, tipo = "ZonaSinCupo" });

        _context.Database.ExecuteSqlRaw(@"
            INSERT INTO asignacionParqueo (id_vehiculo, id_parqueo, hora_entrada, hora_salida)
            VALUES ({0}, {1}, {2}, NULL);
        ", vehiculo.id_vehiculo, parqueoLibre.id_parqueo, DateTime.Now);

        parqueoLibre.id_estadoP = 2;
        _context.SaveChanges();

        string zonaNombre = _context.ZonaParqueo.First(z => z.id_zona == zonaElegida).nombre_zona;

        return Json(new
        {
            success = true,
            usuario = usuario.nombre,
            carnet = usuario.carnet,
            parqueo = parqueoLibre.id_parqueo,
            zona = zonaNombre,
            hora = DateTime.Now
        });
    }

    // DTOs
    public class ImagenDto { public string ImagenBase64 { get; set; } }
    public class ZonaEstudianteDto { public string carnet { get; set; } public int zona { get; set; } }
}
