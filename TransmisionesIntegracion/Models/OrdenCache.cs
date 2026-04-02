namespace TransmisionesIntegracion.Models
{
    public class OrdenCache
    {
        public int Id { get; set; } // ID de Azure o -1 si es offline
        public int IdCliente { get; set; }
        public string TipoOrden { get; set; } = string.Empty; // Factura o Cotizacion
        public string Estado { get; set; } = string.Empty; // Pendiente, Confirmada, Cancelada
        public DateTime Fecha { get; set; }
        public decimal TotalEstimado { get; set; } // Lo calcularemos localmente
    }
}