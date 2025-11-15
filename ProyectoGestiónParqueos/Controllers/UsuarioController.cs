using Microsoft.AspNetCore.Mvc;
using ProyectoGestiónParqueos.Models;
using ZXing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
    //  LECTURA GENERAL QR – ENTRADA / SALIDA
    // ==========================================================
    [HttpPost]
    public IActionResult LeerQR([FromBody] ImagenDto dto)
    {
        byte[] bytes = Convert.FromBase64String(dto.ImagenBase64.Split(',')[1]);
        using var img = Image.Load<Rgba32>(bytes);
        var result = new BarcodeReaderGeneric().Decode(img);
        if (result == null) return Json(new { success = false });

        string carnet = result.Text;
        var usuario = _context.Usuario.FirstOrDefault(u => u.carnet == carnet);
        if (usuario == null) return Json(new { success = false, tipo = "NoRegistrado" });

        if (usuario.id_tipoUsuario == 1 || usuario.id_tipoUsuario == 2)
            return Json(new { success = false, tipo = "SinPermiso" });

        var vehiculo = _context.Vehiculo.FirstOrDefault(v => v.id_user == usuario.id_usuario);
        if (vehiculo == null) return Json(new { success = false, tipo = "NoTieneVehiculo" });

        // --- SALIDA ---
        var asignacionesActivas = _context.AsignacionParqueo
            .Where(a => a.id_vehiculo == vehiculo.id_vehiculo && a.Hora_Salida == null)
            .ToList();

        if (asignacionesActivas.Any())
        {
            foreach (var asig in asignacionesActivas)
            {
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

        int tipo = usuario.id_tipoUsuario;

        // --- DOCENTE = Zona D ---
        if (tipo == 3)
        {
            var parqueoLibre = _context.Parqueo
                .FirstOrDefault(p => p.id_zona == 4 && p.id_estadoP == 1);

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
                zona = "Zona D"
            });
        }

        // --- INVITADO (no se registra nada, solo pasa) ---
        if (tipo == 5)
        {
            return Json(new
            {
                success = true,
                tipo = "Invitado",
                usuario = usuario.nombre,
                carnet = usuario.carnet
            });
        }

        // --- ESTUDIANTE ---
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
    //  ASIGNAR ZONA A ESTUDIANTE  (AQUÍ VA LA MEJORA)
    // ==========================================================
    [HttpPost]
    public IActionResult AsignarZonaEstudiante([FromBody] ZonaEstudianteDto data)
    {
        var usuario = _context.Usuario.First(u => u.carnet == data.carnet);
        var vehiculo = _context.Vehiculo.First(v => v.id_user == usuario.id_usuario);

        int zonaElegida = data.zona;          // 1 = A, 2 = B, 3 = C
        int tipoVehiculo = vehiculo.id_tipoV; // 1 = Auto, 2 = Moto

        // 1) Determinar zonas compatibles según tipo de vehículo
        //    Auto: A (1) y B (2)
        //    Moto: A (1) y C (3)
        var zonasCompatibles = new List<int>();

        if (tipoVehiculo == 1)        // Auto
            zonasCompatibles.AddRange(new[] { 1, 2 });
        else if (tipoVehiculo == 2)   // Moto
            zonasCompatibles.AddRange(new[] { 1, 3 });

        // 2) Verificar que la zona ELEGIDA sea realmente compatible según BD
        bool compatible = zonasCompatibles.Contains(zonaElegida) &&
                          _context.Parqueo.Any(p =>
                              p.id_zona == zonaElegida &&
                              p.id_tipoV == tipoVehiculo);

        if (!compatible)
        {
            string zonaNombreIncompat = _context.ZonaParqueo
                .First(z => z.id_zona == zonaElegida).nombre_zona;

            return Json(new
            {
                success = false,
                tipo = "Incompatible",
                mensaje = $"La zona {zonaNombreIncompat} no admite su tipo de vehículo."
            });
        }

        // 3) Intentar asignar en la zona ELEGIDA (filtrando por tipo de vehículo)
        var parqueoLibre = _context.Parqueo
            .Where(p => p.id_zona == zonaElegida &&
                        p.id_estadoP == 1 &&
                        p.id_tipoV == tipoVehiculo)
            .OrderBy(p => p.id_parqueo)
            .FirstOrDefault();

        // 4) Si la zona elegida está llena → buscar OTRA zona compatible
        if (parqueoLibre == null)
        {
            foreach (var zonaAlt in zonasCompatibles)
            {
                if (zonaAlt == zonaElegida) continue; // ya la intentamos

                var parqueoAlt = _context.Parqueo
                    .Where(p => p.id_zona == zonaAlt &&
                                p.id_estadoP == 1 &&
                                p.id_tipoV == tipoVehiculo)
                    .OrderBy(p => p.id_parqueo)
                    .FirstOrDefault();

                if (parqueoAlt != null)
                {
                    parqueoLibre = parqueoAlt;
                    zonaElegida = zonaAlt; // actualizamos a la nueva zona asignada
                    break;
                }
            }

            // 5) Si NO hay espacios libres en ninguna zona compatible
            if (parqueoLibre == null)
            {
                string zonasTexto = string.Join(" / ",
                    zonasCompatibles.Select(z => _context.ZonaParqueo
                        .First(zo => zo.id_zona == z).nombre_zona));

                return Json(new
                {
                    success = false,
                    tipo = "SinCupo",
                    mensaje = $"No hay espacios disponibles en ninguna zona compatible ({zonasTexto})."
                });
            }
        }

        // 6) Asignar el parqueo encontrado (zona elegida o alternativa)
        _context.Database.ExecuteSqlRaw(@"
            INSERT INTO asignacionParqueo (id_vehiculo, id_parqueo, hora_entrada, hora_salida)
            VALUES ({0}, {1}, {2}, NULL);
        ", vehiculo.id_vehiculo, parqueoLibre.id_parqueo, DateTime.Now);

        parqueoLibre.id_estadoP = 2;
        _context.SaveChanges();

        string nombreZonaFinal = _context.ZonaParqueo
            .First(z => z.id_zona == zonaElegida).nombre_zona;

        return Json(new
        {
            success = true,
            usuario = usuario.nombre,
            carnet = usuario.carnet,
            parqueo = parqueoLibre.id_parqueo,
            zona = nombreZonaFinal,
            hora = DateTime.Now
        });
    }

    public class ImagenDto { public string ImagenBase64 { get; set; } }
    public class ZonaEstudianteDto { public string carnet { get; set; } public int zona { get; set; } }
}
