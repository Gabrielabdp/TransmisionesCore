using System.Text.Json.Serialization;

namespace TransmisionesCore.Entities;
public class Provincia
{
    public int Id_provincia { get; set; }
    public string Nombre_provincia { get; set; } = string.Empty;

    [JsonIgnore] // daba un error de un "ciclo infinito" y esto ayuda a probar los endopints sin errores
    public ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
