using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransmisionesCore.Entities;
public class Sector
{
    [Key]
    public int Id_sector { get; set; }

    public int Id_municipio { get; set; }
    public string Nombre_sector { get; set; } = string.Empty;

    [ForeignKey("Id_municipio")] 
    public virtual Municipio Municipio { get; set; } = null!;

    }
