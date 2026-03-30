namespace TransmisionesCore.Entities;
public class Sucursal
{
    public int Id_sucursal { get; set; }
    public int Id_municipio { get; set; }
    public string Nombre_sucursal { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;
    public Municipio Municipio { get; set; } = null!;
}
