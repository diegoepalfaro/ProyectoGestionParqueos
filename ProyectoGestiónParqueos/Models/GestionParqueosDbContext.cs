using Microsoft.EntityFrameworkCore;
using ProyectoGestiónParqueos.Models;
namespace ProyectoGestiónParqueos.Models
{
    public class GestionParqueosDbContext : DbContext
    {
        public GestionParqueosDbContext(DbContextOptions options) : base(options) { }
        public DbSet<parqueo> Parqueo { get; set; }
        public DbSet<asignacionParqueo> AsignacionParqueo { get; set; }
        public DbSet<reporteParqueo> ReporteParqueo { get; set; }
        public DbSet<estadoParqueo> EstadoParqueo { get; set; }
        public DbSet<Vehiculo> Vehiculo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
    }
}
