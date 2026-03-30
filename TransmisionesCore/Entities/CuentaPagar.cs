namespace TransmisionesCore.Entities;
public class CuentaPagar
{
    public int Id_cpagar { get; set; }
    public int Id_orden_compra { get; set; }
    public string? RNC_proveedor { get; set; }
    public DateTime? Fecha_pago { get; set; }
    public int? Plazo_pago { get; set; }
    public decimal Total_cpagar { get; set; }
    public decimal? Pendiente_cpagar { get; set; }
    public string Estado_cpagar { get; set; } = "Pendiente";
    public OrdenCompra OrdenCompra { get; set; } = null!;
}
