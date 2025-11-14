using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProyectoGestiónParqueos.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoGestiónParqueos.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly GestionParqueosDbContext _context;

        public AdministradorController(GestionParqueosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            string hash = ObtenerHash(password);

            var usuario = await _context.Set<Usuario>()
                .FirstOrDefaultAsync(u => u.correo == correo && u.password_hash == hash);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.id_usuario);
                HttpContext.Session.SetString("NombreUsuario", usuario.nombre);
                HttpContext.Session.SetInt32("Rol", usuario.id_tipoUsuario);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas.";
            return View();
        }

        private string ObtenerHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Administrador");
        }
    }
}
