using TransmisionesCore.Entities;
using TransmisionesCore.UseCases;
using static TransmisionesCore.UseCases.ClienteUseCases;

namespace TransmisionesCore.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(string? buscar = null);
    Task<Cliente> InsertarAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);

    Task<Cliente?> ObtenerPorDocumentoAsync(string documento);

    Task<ClienteResumenDTO?> ObtenerResumenAsync(int id);
}

public interface IProductoRepository
{

    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Producto>> ObtenerTodosAsync(int? idCategoria = null, bool soloConStock = false);
    Task<Producto> InsertarAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    Task<int> AjustarInventarioAsync(AjustarStockRequest request);
    Task<IEnumerable<ProductoFiltroDTO>> ObtenerPorCategoriaAsync(int idCategoria);
    Task<IEnumerable<ProductoRankingDTO>> ObtenerRankingUsoAsync(int top = 10);
    Task<IEnumerable<Producto>> ObtenerBajoStockDesdeSPAsync(int limite);
}

public interface IOrdenRepository
{
    Task<Orden?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Orden>> ObtenerTodosAsync(string? estado = null, int? idCliente = null);
    Task<IEnumerable<Orden>> ObtenerCotizacionesAsync(int? idCliente = null);
    Task<Orden> InsertarAsync(Orden orden);
    Task ActualizarAsync(Orden orden);
}

public interface IFacturaRepository
{
    Task<Factura?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Factura>> ObtenerTodosAsync(DateTime? desde = null, DateTime? hasta = null);
    Task<Factura> InsertarAsync(Factura factura);
    Task<string> GenerarNumeroFacturaAsync();
    Task ActualizarAsync(Factura factura);
}

public interface ICajaRepository
{
    Task<Caja?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Caja>> ObtenerTodosAsync(int? idSucursal = null);
    Task ActualizarAsync(Caja caja);
    Task<decimal> ObtenerVentasDelDiaAsync(DateTime fecha);
    Task<EstadoCajaDTO> ObtenerEstadoActualAsync(int id);
}

public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Proveedor>> ObtenerTodosAsync();
    Task<Proveedor> InsertarAsync(Proveedor proveedor);
    Task ActualizarAsync(Proveedor proveedor);
}

public interface IOrdenCompraRepository
{
    Task<OrdenCompra?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<OrdenCompra>> ObtenerTodosAsync();
    Task<OrdenCompra> InsertarAsync(OrdenCompra orden);
    Task ActualizarAsync(OrdenCompra orden);
}

public interface IEmpleadoRepository
{
    Task<Empleado?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Empleado>> ObtenerTodosAsync(int? idSucursal = null);
    Task<Empleado> InsertarAsync(Empleado empleado);
    Task ActualizarAsync(Empleado empleado);
}

public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> LoginAsync(string nombreUsuario, string contrasena);
    Task<Usuario> InsertarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}

public interface IVehiculoRepository
{
    Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula);
    Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente);
    Task<Vehiculo> InsertarAsync(Vehiculo vehiculo);
    Task<HistorialVehiculoDTO?> ObtenerHistorialPorMatriculaAsync(string matricula);
}

public interface IGarantiaRepository
{
    Task<IEnumerable<Garantia>> ObtenerPorClienteAsync(int idCliente);
    Task<Garantia> InsertarAsync(Garantia garantia);
}

public interface IDevolucionRepository
{
    Task<IEnumerable<Devolucion>> ObtenerTodosAsync();
    Task<Devolucion> InsertarAsync(Devolucion devolucion);
    Task ActualizarAsync(Devolucion devolucion);
}
public interface IServicioRepository
{
    Task<Servicio?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Servicio>> ObtenerTodosAsync(bool soloActivos = true);
    Task<Servicio> InsertarAsync(Servicio servicio);
    Task ActualizarAsync(Servicio servicio);
}

public interface ILogRepository
{
    Task<IEnumerable<AuditoriaPrecioDTO>> ObtenerLogPreciosAsync();
    Task<bool> RegistrarLogAsync(Log log);
    
}


