namespace TransmisionesIntegracion.Models
{
    public class ProductoCache
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int StockActual { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}