using Microsoft.AspNetCore.Mvc;
using ProyectoGestiónParqueos.Models;
using ZXing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;

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

    [HttpPost]
    public IActionResult LeerQR([FromBody] ImagenDto dto)
    {
        byte[] imageBytes = Convert.FromBase64String(dto.ImagenBase64.Split(',')[1]);

        using var image = Image.Load<Rgba32>(imageBytes);

        var reader = new BarcodeReaderGeneric();
        var result = reader.Decode(image);

        if (result != null)
            return Json(new { success = true, data = result.Text });

        return Json(new { success = false });
    }

    public class ImagenDto
    {
        public string ImagenBase64 { get; set; }
    }
}
