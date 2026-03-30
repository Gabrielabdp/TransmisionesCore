namespace TransmisionesCore.Entities;
public class Devolucion
{
    public int Id_devolucion { get; set; }
    public int Id_orden { get; set; }
    public int Id_producto { get; set; }
    public int Id_empleado { get; set; }
    public DateTime Fecha_devolucion { get; set; } = DateTime.UtcNow;
    public string? Motivo { get; set; }
    public bool Regresa_inventario { get; set; }
    public decimal? Monto_devuelto { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public Orden Orden { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
}
