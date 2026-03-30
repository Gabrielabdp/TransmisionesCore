namespace TransmisionesCore.Entities;
public class DetalleCompra
{
    public int Id_orden_compra { get; set; }
    public int Id_producto { get; set; }
    public int Cantidad_ordenada { get; set; }
    public decimal Costo_unitario { get; set; }
    public decimal SubTotal { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
