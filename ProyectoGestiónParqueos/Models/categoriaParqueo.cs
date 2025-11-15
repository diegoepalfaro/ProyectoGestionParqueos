using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class categoriaParqueo
    {
        [Key]
        public int id_categoria { get; set; }

        public string nombreTipo { get; set; }

    }
}
