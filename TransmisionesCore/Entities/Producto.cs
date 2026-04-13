using System.ComponentModel.DataAnnotations.Schema;

namespace TransmisionesCore.Entities;

[Table("Producto")]
public class Producto
{
    public int Id_producto { get; set; }
    public int Id_categoria { get; set; }
    public int Id_tipo_trans { get; set; }
    public string Descripcion_producto { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public int Stock_actual { get; set; }
    public decimal Precio_unitario { get; set; }
    public decimal Costo_unitario { get; set; }
    public bool Activo { get; set; } = true;
    public CategoriaProducto Categoria { get; set; } = null!;
    public TipoTransmision TipoTransmision { get; set; } = null!;
    public decimal MargenGanancia => Precio_unitario - Costo_unitario;
    public bool TieneStock() => Stock_actual > 0;
}
