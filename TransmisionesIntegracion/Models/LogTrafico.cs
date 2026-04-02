namespace TransmisionesIntegracion.Models
{
    public class LogTrafico
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string MetodoHttp { get; set; } = string.Empty; // GET, POST
        public string Endpoint { get; set; } = string.Empty; // La URL visitada
        public string PeticionBody { get; set; } = string.Empty; // Lo que mandó la Caja (El JSON)
        public int StatusCode { get; set; } // 200 (Éxito), 500 (Error), 503 (Offline)
        public string RespuestaBody { get; set; } = string.Empty; // Lo que tú le respondiste
        public string OrigenIP { get; set; } = string.Empty;
    }
}