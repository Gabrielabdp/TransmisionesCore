namespace TransmisionesCore.Entities;
public class CuentaCobrar
{
    public int Id_ccobro { get; set; }
    public int Id_orden { get; set; }
    public string? RNC_cliente { get; set; }
    public DateTime? Fecha_pago { get; set; }
    public int? Plazo_pago { get; set; }
    public decimal Total_ccobro { get; set; }
    public decimal? Pendiente_ccobro { get; set; }
    public string Estado_ccobro { get; set; } = "Pendiente";
    public Orden Orden { get; set; } = null!;
}
