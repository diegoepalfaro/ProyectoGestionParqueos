namespace ProyectoGestiónParqueos.Models
{
    public class zonaParqueo
    {
        public int id_zona { get; set; }

        public int id_categoria { get; set; }

        public string nombre_zona { get; set; }

        public int capacidadMax { get; set; }
    }
}
