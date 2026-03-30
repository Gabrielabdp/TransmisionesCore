namespace TransmisionesCore.Entities;
public class Orden
{
    public int Id_orden { get; set; }
    public int Id_cliente { get; set; }
    public int Id_empleado { get; set; }
    public int? Id_condicion_pago { get; set; }
    public int Id_canal { get; set; }
    public string? Id_vehiculo { get; set; }
    public DateTime Fecha_orden { get; set; } = DateTime.UtcNow;
    public string Tipo_orden { get; set; } = "Venta";
    public string Estado_orden { get; set; } = "Cotizacion";
    public DateTime? Fecha_vencimiento_cotizacion { get; set; }
    public decimal? Total_orden { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public CanalVenta Canal { get; set; } = null!;
    public Vehiculo? Vehiculo { get; set; }
    public ICollection<DetalleOrden> DetallesOrden { get; set; } = new List<DetalleOrden>();
    public ICollection<DetalleServicio> DetallesServicio { get; set; } = new List<DetalleServicio>();
    public bool EsCotizacion() => Estado_orden == "Cotizacion";
    public bool PuedeConfirmarse() => Estado_orden is "Cotizacion" or "Pendiente";
    public bool PuedeCancelarse() => Estado_orden != "Entregada";
}
