namespace TransmisionesCore.Entities;
public class Proveedor
{
    public int Id_proveedor { get; set; }
    public int Id_sector { get; set; }
    public int Id_municipio { get; set; }
    public int Id_provincia { get; set; }
    public string Nombre_proveedor { get; set; } = string.Empty;
    public string? Apellido_proveedor { get; set; }
    public string? RNC_proveedor { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Activo { get; set; } = true;
    public Sector Sector { get; set; } = null!;
    public Municipio Municipio { get; set; } = null!;
    public Provincia Provincia { get; set; } = null!;
}
