using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGestiónParqueos.Models
{
    [Table("parqueo")]
    public class parqueo
    {
        [Key]
        public int id_parqueo { get; set; }

        public int id_estadoP { get; set; }
        public int id_zona { get; set; }
        public int id_tipoV { get; set; }

        [ForeignKey("id_estadoP")]
        public estadoParqueo Estado { get; set; }

        [ForeignKey("id_zona")]
        public zonaParqueo Zona { get; set; }

        [ForeignKey("id_tipoV")]
        public tipoVehiculo TipoVehiculo { get; set; }
    }
}
