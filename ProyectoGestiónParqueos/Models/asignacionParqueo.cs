using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class asignacionParqueo
    {
        [Key]
        public int id_asignacionP { get; set; }

        public int id_vehiculo { get; set; }

        public int id_parqueo { get; set; }

        public DateTime Hora_Entrada { get; set; }

        public DateTime Hora_Salida { get; set; }
    }
}
