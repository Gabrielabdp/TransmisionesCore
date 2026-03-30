namespace TransmisionesCore.Entities;
public class Servicio
{
    public int Id_servicio { get; set; }
    public int Id_tipo_servicio { get; set; }
    public string Nombre_servicio { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio_base { get; set; }
    public bool Activo { get; set; } = true;
    public TipoServicio TipoServicio { get; set; } = null!;
}
