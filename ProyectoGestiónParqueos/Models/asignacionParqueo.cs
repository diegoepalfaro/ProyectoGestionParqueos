using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGestiónParqueos.Models
{
    public class asignacionParqueo
    {
        [Key]
        public int id_asignacionP { get; set; }

        public int id_vehiculo { get; set; }

        public int id_parqueo { get; set; }

        public DateTime Hora_Entrada { get; set; }

        public DateTime? Hora_Salida { get; set; }

        [ForeignKey("id_vehiculo")]
        public Vehiculo Vehiculo { get; set; }

        [ForeignKey("id_parqueo")]
        public parqueo Parqueo { get; set; }


    }
}
