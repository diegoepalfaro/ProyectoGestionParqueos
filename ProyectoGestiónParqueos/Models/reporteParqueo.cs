using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class reporteParqueo
    {
        [Key]
        public int id_reporte { get; set; }

        public int id_asignacionP { get; set; }

        public int id_usuario { get; set; }

        public string descripcionR { get; set; }

        public DateTime fechaHoraR { get; set; }

        public string estadoR { get; set; }
    }
}
