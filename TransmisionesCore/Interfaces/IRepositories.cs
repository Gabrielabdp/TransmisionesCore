using TransmisionesCore.Entities;
using TransmisionesCore.UseCases;

namespace TransmisionesCore.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(string? buscar = null);
    Task<Cliente> InsertarAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);
}

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Producto>> ObtenerTodosAsync(int? idCategoria = null, bool soloConStock = false);
    Task<Producto> InsertarAsync(Producto producto);
    Task ActualizarAsync(Producto producto);

    Task<int> AjustarInventarioAsync(AjustarStockRequest request);
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
}

public interface ICajaRepository
{
    Task<Caja?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Caja>> ObtenerTodosAsync(int? idSucursal = null);
    Task ActualizarAsync(Caja caja);
    Task RegistrarMovimientoAsync(MovimientoCaja movimiento);
    Task<IEnumerable<MovimientoCaja>> ObtenerMovimientosAsync(int idCaja, DateTime desde);
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
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> LoginAsync(string email, string contrasena);
    Task<Usuario> InsertarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}

public interface IVehiculoRepository
{
    Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula);
    Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente);
    Task<Vehiculo> InsertarAsync(Vehiculo vehiculo);
    Task EliminarAsync(string matricula);
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


