namespace TransmisionesCore.Entities;
public class Vehiculo
{
    public string Matricula { get; set; } = string.Empty;
    public int Id_cliente { get; set; }
    public int Id_tipo_trans { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public short Ano { get; set; }
    public string? Color { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public TipoTransmision TipoTransmision { get; set; } = null!;
}
