namespace TransmisionesCore.Entities;
public class Factura
{
    public int Id_factura { get; set; }
    public int Id_orden { get; set; }
    public int Id_cliente { get; set; }
    public int Id_empleado { get; set; }
    public int Id_caja { get; set; }
    public string Numero_factura { get; set; } = string.Empty;
    public DateTime Fecha_factura { get; set; } = DateTime.UtcNow;
    public string? Tipo_factura { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? ITBIS { get; set; }
    public decimal? Total { get; set; }
    public string Estado { get; set; } = "Emitida";
    public Orden Orden { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
}
