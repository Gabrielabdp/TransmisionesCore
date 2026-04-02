namespace TransmisionesIntegracion.Models
{
    public class ClienteCache
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty; 
        public string Telefono { get; set; } = string.Empty;
    }
}