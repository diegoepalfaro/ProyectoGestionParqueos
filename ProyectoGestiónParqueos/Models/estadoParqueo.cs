using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class estadoParqueo
    {
        [Key]
        public int id_estadoParqueo {  get; set; }
        public string nombreEstado {  get; set; }

    }
}
