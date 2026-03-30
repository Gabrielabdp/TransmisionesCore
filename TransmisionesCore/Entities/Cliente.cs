using System.ComponentModel.DataAnnotations.Schema;

namespace TransmisionesCore.Entities;

[Table("Cliente")]
public class Cliente
{
    public int Id_cliente { get; set; }
    public int Id_sector { get; set; }
    public int Id_municipio { get; set; }
    public int Id_provincia { get; set; }
    public string? RNC_cliente { get; set; }
    public string? Cedula_cliente { get; set; }
    public string Nombre_cliente { get; set; } = string.Empty;
    public string Apellido_cliente { get; set; } = string.Empty;
    public string? Telefono_cliente { get; set; }
    public string? Correo_cliente { get; set; }
    public DateTime Fecha_registro { get; set; } = DateTime.UtcNow;

    public Sector Sector { get; set; } = null!;

    public Municipio Municipio { get; set; } = null!;

    public Provincia Provincia { get; set; } = null!;
    public string NombreCompleto => $"{Nombre_cliente} {Apellido_cliente}";
    public bool EsAnonimo() => Id_cliente == 1;
}
