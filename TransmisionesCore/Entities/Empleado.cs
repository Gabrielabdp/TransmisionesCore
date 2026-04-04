namespace TransmisionesCore.Entities;
public class Empleado
{
    public int Id_empleado { get; set; }
    public int Id_usuario { get; set; }
    public int Id_sucursal { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime Fecha_ingreso { get; set; }
    public bool Activo { get; set; } = true;
 
    public Usuario? Usuario { get; set; } = null!;
    public Sucursal? Sucursal { get; set; } = null!;
    public string NombreCompleto => $"{Nombre} {Apellido}";
}
