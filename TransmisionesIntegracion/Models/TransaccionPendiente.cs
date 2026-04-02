namespace TransmisionesIntegracion.Models
{
    public class TransaccionPendiente
    {
        public int Id { get; set; }
        public string TipoTransaccion { get; set; } = string.Empty; 
        public string DatosJson { get; set; } = string.Empty;
        public DateTime FechaIntento { get; set; }
        public bool Sincronizado { get; set; } = false;
    }
}