namespace TransmisionesCore.Entities;
public class AjusteInventario
{
    public int Id_ajuste { get; set; }
    public int Id_producto { get; set; }
    public int Id_empleado { get; set; }
    public string Tipo_ajuste { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int Stock_anterior { get; set; }
    public int Stock_nuevo { get; set; }
    public Producto Producto { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
}
