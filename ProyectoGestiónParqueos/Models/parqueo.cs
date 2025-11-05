using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class parqueo
    {
        [Key]
        public int id_parqueo { get; set; }

        public int id_estadoP { get; set; }

        public int id_zona { get; set; }

        public int id_tipoV { get; set; }
    }
}
