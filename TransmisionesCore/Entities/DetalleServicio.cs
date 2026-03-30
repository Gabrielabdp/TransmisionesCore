namespace TransmisionesCore.Entities;
public class DetalleServicio
{
    public int Id_orden { get; set; }
    public int Id_servicio { get; set; }
    public int Id_empleado_tecnico { get; set; }
    public string? Descripcion_trabajo { get; set; }
    public decimal Precio_cobrado { get; set; }
    public decimal SubTotal { get; set; }
    public Orden Orden { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
    public Empleado EmpleadoTecnico { get; set; } = null!;
}
