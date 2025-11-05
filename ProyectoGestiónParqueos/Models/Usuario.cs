using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ProyectoGestiónParqueos.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario { get; set; }

        public int id_tipoUsuario { get; set; }

        public string nombre { get; set; }

        public string carnet { get; set; }

        public string correo { get; set; }

        public string password_hash { get; set; }


    }
}
