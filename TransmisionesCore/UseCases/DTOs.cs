namespace TransmisionesCore.UseCases;

public record CrearOrdenRequest(
    int IdCliente, int IdEmpleado, int IdCanal,
    string TipoOrden = "Venta", string EstadoOrden = "Cotizacion",
    int? IdCondicionPago = null, string? IdVehiculo = null,
    DateTime? FechaVencimientoCotizacion = null);

public record AgregarProductoOrdenRequest(int IdOrden, int IdProducto, int Cantidad);

public record AgregarServicioOrdenRequest(
    int IdOrden, int IdServicio, int IdEmpleadoTecnico,
    decimal PrecioCobrado, string? DescripcionTrabajo = null);

public record CrearClienteRequest(
    int IdSector, int IdMunicipio, int IdProvincia,
    string NombreCliente, string ApellidoCliente,
    string? RNC = null, string? Cedula = null,
    string? Telefono = null, string? Correo = null);

public record CrearProductoRequest(
    int IdCategoria, int IdTipoTrans, string Descripcion,
    decimal PrecioUnitario, decimal CostoUnitario,
    string? Marca = null, int StockInicial = 0);

public record AjustarStockRequest(
    int IdProducto,
    int IdEmpleado,
    string TipoAjuste,
    int Cantidad,
    string? Motivo = null);

public record ClienteBusquedaDTO(
    int IdCliente,
    string NombreCompleto,
    string Documento,
    string Telefono,
    string? Correo = null);

public record ClienteResumenDTO(
    int IdCliente,
    string NombreCompleto,
    decimal SaldoPendiente,
    int CantidadVehiculos,
    int OrdenesAbiertas,
    DateTime? FechaUltimoServicio
);

public record HistorialVehiculoDTO(
    string Matricula,
    string Vehiculo, // Marca + Modelo
    string Cliente,
    List<ServicioHistorialDTO> Servicios
);

public record ServicioHistorialDTO(
    int IdOrden,
    DateTime Fecha,
    string TipoTrabajo,
    string Descripcion,
    decimal Total,
    string Estado
);

public record ProductoFiltroDTO(
    int IdProducto,
    string Descripcion,
    string Marca,
    decimal Precio,
    int Stock,
    string CategoriaNombre
);

public record AsignarEmpleadoRequest(int IdEmpleado);

public record ActualizarPrecioItem(int IdProducto, decimal NuevoPrecio);
public record ActualizarPreciosLoteRequest(List<ActualizarPrecioItem> Precios);

public record ResumenCajaDiarioDTO(
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal SaldoNeto,
    int CantidadOperaciones,
    DateTime Fecha
);

public record ProductoRankingDTO(
    int IdProducto,
    string Nombre,
    int CantidadUtilizada,
    decimal TotalGenerado
);

public record ResumenFlotaDTO(
    int TotalVehiculos,
    List<EstadoVehiculoCount> DistribucionEstados
);

public record EstadoVehiculoCount(string Estado, int Cantidad);
public record GarantiaEstadoDTO(
    bool TieneGarantiaActiva,
    DateTime? FechaVencimiento,
    string Mensaje,
    string? FolioOrdenOriginal
);
public record ProductoBajoStockDTO(
    int IdProducto,
    string Nombre,
    int StockActual,
    int StockMinimo,
    string Mensaje
);
public class EstadoCajaDTO
{
    public int Id_caja { get; set; }
    public string Codigo_caja { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Saldo_inicial { get; set; }
    public decimal Total_Entradas { get; set; } // Suma de Cobros
    public decimal Total_Salidas { get; set; }  // Suma de Pagos
    public decimal Balance_Calculado { get; set; }
}

public class AuditoriaPrecioDTO
{
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}