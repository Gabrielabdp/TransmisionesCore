namespace TransmisionesCore.Entities;
public class OrdenCompra
{
    public int Id_orden_compra { get; set; }
    public int Id_proveedor { get; set; }
    public int Id_empleado { get; set; }
    public int? Id_condicion_pago { get; set; }
    public DateTime Fecha_orden { get; set; } = DateTime.UtcNow;
    public decimal? Total_orden { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
