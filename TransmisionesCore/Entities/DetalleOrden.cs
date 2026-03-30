namespace TransmisionesCore.Entities;
public class DetalleOrden
{
    public int Id_orden { get; set; }
    public int Id_producto { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio_unitario { get; set; }
    public decimal SubTotal { get; set; }
    public Orden Orden { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
