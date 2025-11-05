using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoGestiónParqueos.Models
{
    public class tipoUsuario
    {
        [Key]
        public int id_tipoUsuario { get; set; }
        public string nombreTipoU {  get; set; }
    }
}
