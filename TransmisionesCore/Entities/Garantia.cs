namespace TransmisionesCore.Entities;
public class Garantia
{
    public int Id_garantia { get; set; }
    public int Id_orden { get; set; }
    public int Id_producto { get; set; }
    public int Id_cliente { get; set; }
    public DateTime Fecha_inicio { get; set; }
    public DateTime Fecha_fin { get; set; }
    public int Meses_garantia { get; set; }
    public string Estado { get; set; } = "Activa";
    public Orden Orden { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public bool EstaVigente() => DateTime.UtcNow <= Fecha_fin;
}
