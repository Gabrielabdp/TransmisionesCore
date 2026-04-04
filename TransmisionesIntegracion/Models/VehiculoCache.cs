namespace TransmisionesIntegracion.Models
{
    public class VehiculoCache
    {
        public string Matricula { get; set; } = string.Empty; // La matrícula suele ser la llave primaria
        public int IdCliente { get; set; }
        public int IdTipoTrans { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Color { get; set; } = string.Empty;
        public DateTime UltimaActualizacion { get; set; }
    }
}