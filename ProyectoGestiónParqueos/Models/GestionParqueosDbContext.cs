using Microsoft.EntityFrameworkCore;
using ProyectoGestiónParqueos.Models;
namespace ProyectoGestiónParqueos.Models
{
    public class GestionParqueosDbContext : DbContext
    {

        public GestionParqueosDbContext(DbContextOptions options) : base(options)
        {

        }

    }
}
