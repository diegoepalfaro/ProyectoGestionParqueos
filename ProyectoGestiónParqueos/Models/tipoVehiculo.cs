using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class tipoVehiculo
    {
        [Key]
        public int id_tipoVehiculo { get; set; }

        public int nombreTipo {  get; set; }
    }
}
