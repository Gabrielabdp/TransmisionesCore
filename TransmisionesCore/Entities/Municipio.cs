namespace TransmisionesCore.Entities;

public class Municipio
{
    public int Id_municipio { get; set; }
    public string Nombre_municipio { get; set; } = string.Empty;
    public int Id_provincia { get; set; }
    public Provincia Provincia { get; set; } = null;
    public ICollection<Sector> Sectores { get; set; } = new List<Sector>();
}