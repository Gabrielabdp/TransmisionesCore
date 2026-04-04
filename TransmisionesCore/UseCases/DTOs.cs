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
