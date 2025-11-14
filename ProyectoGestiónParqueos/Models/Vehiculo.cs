using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class Vehiculo
    {
        [Key]
        public int id_vehiculo { get; set; }
        public int id_user { get; set; }
        public int id_tipoV { get; set; }
        public string placasV { get; set; }

    }
}
