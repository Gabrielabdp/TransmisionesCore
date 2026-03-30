namespace TransmisionesCore.Entities;
public class Pago
{
    public int Id_pago { get; set; }
    public int Id_cpagar { get; set; }
    public int Id_metodopago { get; set; }
    public int Id_caja { get; set; }
    public int Id_usuario { get; set; }
    public DateTime Fecha_pago { get; set; } = DateTime.UtcNow;
    public decimal Monto_pago { get; set; }
    public CuentaPagar CuentaPagar { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
