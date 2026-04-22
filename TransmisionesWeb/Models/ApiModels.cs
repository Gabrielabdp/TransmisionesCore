using System.Text.Json.Serialization;

namespace TransmisionesWeb.Models;

public record EstadoSistemaDto(bool AzureDisponible, int TransaccionesPendientes, DateTime Timestamp);

public record ClienteDto(
    [property: JsonPropertyName("idCliente")] int Id,
    [property: JsonPropertyName("nombreCompleto")] string Nombre,
    string Apellido,
    string Documento,
    string? Telefono,
    string? Correo
);

public record OrdenDto(
    int Id_orden,
    string Estado,
    DateTime Fecha_ingreso,
    string? Matricula_vehiculo,
    string? Modelo_vehiculo,
    int Id_cliente
);

public record DetalleFacturaDto(
    string Descripcion,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record FacturaDto(
    int Id_factura,
    string Numero_factura,
    DateTime Fecha_emision,
    decimal Subtotal,
    decimal ITBIS,
    decimal Total,
    string Tipo_factura,
    int Id_cliente,
    string NombreCliente,
    string? CorreoCliente,
    List<DetalleFacturaDto> Detalles
);

public record LoginResponseDto(
    [property: JsonPropertyName("id_empleado")] int IdEmpleado,
    [property: JsonPropertyName("nombreCompleto")] string? Nombre,
    string? Rol,
    [property: JsonPropertyName("id_sucursal")] int? IdSucursal,
    [property: JsonPropertyName("sucursal")] string? NombreSucursal,
    bool ModoOffline
);

public record EstadoCajaDto(
    string CodigoCaja,
    string Estado,
    decimal SaldoInicial,
    decimal SaldoFinal
);

public record ResumenCajaDiarioDto(
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal SaldoNeto,
    int CantidadOperaciones,
    DateTime Fecha
);

public record ProductoDto(
    int Id_producto,
    string Nombre_producto,
    decimal Precio_venta,
    int Stock_actual,
    string? Categoria
);

public record ServicioDto(
    int Id_servicio,
    string Nombre_servicio,
    decimal Precio_base
);

public record ProcesarOrdenResponseDto(
    int? IdFactura,
    int? IdOrden,
    bool Exito,
    string? Mensaje
);

public class ItemCotizacion
{
    public int Id         { get; set; }
    public string Nombre  { get; set; } = string.Empty;
    public string Tipo    { get; set; } = string.Empty;
    public int Cantidad   { get; set; } = 1;
    public decimal Precio { get; set; }
    public decimal Subtotal => Cantidad * Precio;
}
