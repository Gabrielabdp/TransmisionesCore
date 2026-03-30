Eres un asistente experto en C# y .NET 8. Necesito que crees la estructura completa de archivos para una solución llamada TransmisionesSolution que ya existe en Visual Studio con 3 proyectos: TransmisionesCore, TransmisionesInfraestructura y TransmisionesAPI.

La solución está en esta ruta: C:\Users\MARKET\OneDrive - INTEC\INTEC\Trimestre 7\Desarrollo 3\TransmisionesSolution

INSTRUCCIONES:
- Crea cada archivo .cs en su carpeta correcta
- Usa los namespaces exactos que te indico
- No modifiques nada que ya exista, solo crea archivos nuevos
- Si la carpeta no existe, créala

=============================================================
PROYECTO 1: TransmisionesCore
=============================================================

CARPETA: TransmisionesCore/Entities/

Crea el archivo Provincia.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Provincia
{
    public int Id_provincia { get; set; }
    public string Nombre_provincia { get; set; } = string.Empty;
    public ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
```

Crea el archivo Municipio.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Municipio
{
    public int Id_municipio { get; set; }
    public int Id_provincia { get; set; }
    public string Nombre_municipio { get; set; } = string.Empty;
    public Provincia Provincia { get; set; } = null!;
    public ICollection<Sector> Sectores { get; set; } = new List<Sector>();
}
```

Crea el archivo Sector.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Sector
{
    public int Id_sector { get; set; }
    public int Id_municipio { get; set; }
    public string Nombre_sector { get; set; } = string.Empty;
    public Municipio Municipio { get; set; } = null!;
}
```

Crea el archivo TipoTransmision.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class TipoTransmision
{
    public int Id_tipo_trans { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
```

Crea el archivo CategoriaProducto.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class CategoriaProducto
{
    public int Id_categoria { get; set; }
    public string Nombre_categoria { get; set; } = string.Empty;
}
```

Crea el archivo TipoServicio.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class TipoServicio
{
    public int Id_tipo_servicio { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
```

Crea el archivo CondicionesPago.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class CondicionesPago
{
    public int Id_condicion_pago { get; set; }
    public int? Plazo_pago { get; set; }
    public string? Descripcion { get; set; }
}
```

Crea el archivo MetodoPago.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class MetodoPago
{
    public int Id_metodopago { get; set; }
    public string Descripcion_metodo { get; set; } = string.Empty;
}
```

Crea el archivo CanalVenta.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class CanalVenta
{
    public int Id_canal { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
```

Crea el archivo Usuario.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Usuario
{
    public int Id_usuario { get; set; }
    public string Nombre_usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
```

Crea el archivo Sucursal.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Sucursal
{
    public int Id_sucursal { get; set; }
    public int Id_municipio { get; set; }
    public string Nombre_sucursal { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;
    public Municipio Municipio { get; set; } = null!;
}
```

Crea el archivo Empleado.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Empleado
{
    public int Id_empleado { get; set; }
    public int Id_usuario { get; set; }
    public int Id_sucursal { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime Fecha_ingreso { get; set; }
    public bool Activo { get; set; } = true;
    public Usuario Usuario { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
    public string NombreCompleto => $"{Nombre} {Apellido}";
}
```

Crea el archivo Caja.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Caja
{
    public int Id_caja { get; set; }
    public int Id_sucursal { get; set; }
    public int? Id_usuario_apertura { get; set; }
    public int? Id_usuario_cierre { get; set; }
    public string Codigo_caja { get; set; } = string.Empty;
    public string Estado { get; set; } = "Cerrada";
    public decimal Saldo_inicial { get; set; }
    public decimal Saldo_final { get; set; }
    public string? Tipo_movimiento { get; set; }
    public DateTime? Ultima_apertura { get; set; }
    public DateTime? Ultimo_cierre { get; set; }
    public bool Activa { get; set; } = true;
    public Sucursal Sucursal { get; set; } = null!;
    public Usuario? UsuarioApertura { get; set; }
    public Usuario? UsuarioCierre { get; set; }
}
```

Crea el archivo Producto.cs con este contenido:
```
namespace TransmisionesCore.Entities;
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
```

Crea el archivo Servicio.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Servicio
{
    public int Id_servicio { get; set; }
    public int Id_tipo_servicio { get; set; }
    public string Nombre_servicio { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio_base { get; set; }
    public bool Activo { get; set; } = true;
    public TipoServicio TipoServicio { get; set; } = null!;
}
```

Crea el archivo Proveedor.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Proveedor
{
    public int Id_proveedor { get; set; }
    public int Id_sector { get; set; }
    public int Id_municipio { get; set; }
    public int Id_provincia { get; set; }
    public string Nombre_proveedor { get; set; } = string.Empty;
    public string? Apellido_proveedor { get; set; }
    public string? RNC_proveedor { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public bool Activo { get; set; } = true;
    public Sector Sector { get; set; } = null!;
    public Municipio Municipio { get; set; } = null!;
    public Provincia Provincia { get; set; } = null!;
}
```

Crea el archivo Cliente.cs con este contenido:
```
namespace TransmisionesCore.Entities;
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
```

Crea el archivo Vehiculo.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Vehiculo
{
    public string Matricula { get; set; } = string.Empty;
    public int Id_cliente { get; set; }
    public int Id_tipo_trans { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public short Ano { get; set; }
    public string? Color { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public TipoTransmision TipoTransmision { get; set; } = null!;
}
```

Crea el archivo Orden.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Orden
{
    public int Id_orden { get; set; }
    public int Id_cliente { get; set; }
    public int Id_empleado { get; set; }
    public int? Id_condicion_pago { get; set; }
    public int Id_canal { get; set; }
    public string? Id_vehiculo { get; set; }
    public DateTime Fecha_orden { get; set; } = DateTime.UtcNow;
    public string Tipo_orden { get; set; } = "Venta";
    public string Estado_orden { get; set; } = "Cotizacion";
    public DateTime? Fecha_vencimiento_cotizacion { get; set; }
    public decimal? Total_orden { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public CanalVenta Canal { get; set; } = null!;
    public Vehiculo? Vehiculo { get; set; }
    public ICollection<DetalleOrden> DetallesOrden { get; set; } = new List<DetalleOrden>();
    public ICollection<DetalleServicio> DetallesServicio { get; set; } = new List<DetalleServicio>();
    public bool EsCotizacion() => Estado_orden == "Cotizacion";
    public bool PuedeConfirmarse() => Estado_orden is "Cotizacion" or "Pendiente";
    public bool PuedeCancelarse() => Estado_orden != "Entregada";
}
```

Crea el archivo DetalleOrden.cs con este contenido:
```
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
```

Crea el archivo DetalleServicio.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class DetalleServicio
{
    public int Id_orden { get; set; }
    public int Id_servicio { get; set; }
    public int Id_empleado_tecnico { get; set; }
    public string? Descripcion_trabajo { get; set; }
    public decimal Precio_cobrado { get; set; }
    public decimal SubTotal { get; set; }
    public Orden Orden { get; set; } = null!;
    public Servicio Servicio { get; set; } = null!;
    public Empleado EmpleadoTecnico { get; set; } = null!;
}
```

Crea el archivo Factura.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Factura
{
    public int Id_factura { get; set; }
    public int Id_orden { get; set; }
    public int Id_cliente { get; set; }
    public int Id_empleado { get; set; }
    public int Id_caja { get; set; }
    public string Numero_factura { get; set; } = string.Empty;
    public DateTime Fecha_factura { get; set; } = DateTime.UtcNow;
    public string? Tipo_factura { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? ITBIS { get; set; }
    public decimal? Total { get; set; }
    public string Estado { get; set; } = "Emitida";
    public Orden Orden { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
}
```

Crea el archivo CuentaCobrar.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class CuentaCobrar
{
    public int Id_ccobro { get; set; }
    public int Id_orden { get; set; }
    public string? RNC_cliente { get; set; }
    public DateTime? Fecha_pago { get; set; }
    public int? Plazo_pago { get; set; }
    public decimal Total_ccobro { get; set; }
    public decimal? Pendiente_ccobro { get; set; }
    public string Estado_ccobro { get; set; } = "Pendiente";
    public Orden Orden { get; set; } = null!;
}
```

Crea el archivo Cobro.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Cobro
{
    public int Id_cobro { get; set; }
    public int Id_ccobro { get; set; }
    public int Id_metodopago { get; set; }
    public int Id_caja { get; set; }
    public int Id_usuario { get; set; }
    public DateTime Fecha_pago { get; set; } = DateTime.UtcNow;
    public decimal Monto_pago { get; set; }
    public CuentaCobrar CuentaCobrar { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
```

Crea el archivo OrdenCompra.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class OrdenCompra
{
    public int Id_orden_compra { get; set; }
    public int Id_proveedor { get; set; }
    public int Id_empleado { get; set; }
    public int? Id_condicion_pago { get; set; }
    public DateTime Fecha_orden { get; set; } = DateTime.UtcNow;
    public decimal? Total_orden { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
```

Crea el archivo DetalleCompra.cs con este contenido:
```
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
```

Crea el archivo CuentaPagar.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class CuentaPagar
{
    public int Id_cpagar { get; set; }
    public int Id_orden_compra { get; set; }
    public string? RNC_proveedor { get; set; }
    public DateTime? Fecha_pago { get; set; }
    public int? Plazo_pago { get; set; }
    public decimal Total_cpagar { get; set; }
    public decimal? Pendiente_cpagar { get; set; }
    public string Estado_cpagar { get; set; } = "Pendiente";
    public OrdenCompra OrdenCompra { get; set; } = null!;
}
```

Crea el archivo Pago.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Pago
{
    public int Id_pago { get; set; }
    public int Id_cpagar { get; set; }
    public int Id_metodopago { get; set; }
    public int Id_caja { get; set; }
    public int Id_usuario { get; set; }
    public DateTime Fecha_pago { get; set; } = DateTime.UtcNow;
    public decimal Monto_pago { get; set; }
    public CuentaPagar CuentaPagar { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
```

Crea el archivo Garantia.cs con este contenido:
```
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
```

Crea el archivo Devolucion.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Devolucion
{
    public int Id_devolucion { get; set; }
    public int Id_orden { get; set; }
    public int Id_producto { get; set; }
    public int Id_empleado { get; set; }
    public DateTime Fecha_devolucion { get; set; } = DateTime.UtcNow;
    public string? Motivo { get; set; }
    public bool Regresa_inventario { get; set; }
    public decimal? Monto_devuelto { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public Orden Orden { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
}
```

Crea el archivo AjusteInventario.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class AjusteInventario
{
    public int Id_ajuste { get; set; }
    public int Id_producto { get; set; }
    public int Id_empleado { get; set; }
    public string Tipo_ajuste { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int Stock_anterior { get; set; }
    public int Stock_nuevo { get; set; }
    public Producto Producto { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
}
```

Crea el archivo Log.cs con este contenido:
```
namespace TransmisionesCore.Entities;
public class Log
{
    public int Id_log { get; set; }
    public int Id_usuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? Tabla_afectada { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Usuario Usuario { get; set; } = null!;
}
```

---

CARPETA: TransmisionesCore/Exceptions/

Crea el archivo DomainExceptions.cs con este contenido:
```
namespace TransmisionesCore.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string producto)
        : base($"Stock insuficiente para: {producto}") { }
}

public class OrdenNoConfirmableException : DomainException
{
    public OrdenNoConfirmableException(int id)
        : base($"La orden {id} no puede confirmarse en su estado actual.") { }
}

public class CajaYaAbiertaException : DomainException
{
    public CajaYaAbiertaException(string codigo)
        : base($"La caja {codigo} ya está abierta.") { }
}

public class CajaCerradaException : DomainException
{
    public CajaCerradaException(string codigo)
        : base($"La caja {codigo} no está abierta.") { }
}

public class EntidadNoEncontradaException : DomainException
{
    public EntidadNoEncontradaException(string entidad, object id)
        : base($"{entidad} con ID {id} no encontrado(a).") { }
}

public class PrecioInvalidoException : DomainException
{
    public PrecioInvalidoException()
        : base("El precio de venta no puede ser menor al costo.") { }
}

public class FacturaSinOrdenConfirmadaException : DomainException
{
    public FacturaSinOrdenConfirmadaException()
        : base("Solo se puede facturar una orden confirmada.") { }
}
```

---

CARPETA: TransmisionesCore/Interfaces/

Crea el archivo IRepositories.cs con este contenido:
```
using TransmisionesCore.Entities;

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
    Task<Usuario?> LoginAsync(string nombreUsuario, string contrasena);
    Task<Usuario> InsertarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}

public interface IVehiculoRepository
{
    Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula);
    Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente);
    Task<Vehiculo> InsertarAsync(Vehiculo vehiculo);
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
```

---

CARPETA: TransmisionesCore/UseCases/

Crea el archivo DTOs.cs con este contenido:
```
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
```

Crea el archivo OrdenUseCases.cs con este contenido:
```
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class OrdenUseCases
{
    private readonly IOrdenRepository _ordenRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IClienteRepository _clienteRepo;

    public OrdenUseCases(IOrdenRepository ordenRepo, IProductoRepository productoRepo, IClienteRepository clienteRepo)
    {
        _ordenRepo    = ordenRepo;
        _productoRepo = productoRepo;
        _clienteRepo  = clienteRepo;
    }

    public async Task<Orden> CrearOrdenAsync(CrearOrdenRequest req)
    {
        var cliente = await _clienteRepo.ObtenerPorIdAsync(req.IdCliente)
            ?? throw new EntidadNoEncontradaException("Cliente", req.IdCliente);

        var orden = new Orden
        {
            Id_cliente                   = req.IdCliente,
            Id_empleado                  = req.IdEmpleado,
            Id_canal                     = req.IdCanal,
            Id_condicion_pago            = req.IdCondicionPago,
            Id_vehiculo                  = req.IdVehiculo,
            Tipo_orden                   = req.TipoOrden,
            Estado_orden                 = req.EstadoOrden,
            Fecha_vencimiento_cotizacion = req.FechaVencimientoCotizacion,
            Fecha_orden                  = DateTime.UtcNow,
            Total_orden                  = 0
        };

        return await _ordenRepo.InsertarAsync(orden);
    }

    public async Task AgregarProductoAsync(AgregarProductoOrdenRequest req)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(req.IdOrden)
            ?? throw new EntidadNoEncontradaException("Orden", req.IdOrden);

        var producto = await _productoRepo.ObtenerPorIdAsync(req.IdProducto)
            ?? throw new EntidadNoEncontradaException("Producto", req.IdProducto);

        if (producto.Stock_actual < req.Cantidad)
            throw new StockInsuficienteException(producto.Descripcion_producto);

        var detalle = new DetalleOrden
        {
            Id_orden        = req.IdOrden,
            Id_producto     = req.IdProducto,
            Cantidad        = req.Cantidad,
            Precio_unitario = producto.Precio_unitario,
            SubTotal        = producto.Precio_unitario * req.Cantidad
        };

        orden.DetallesOrden.Add(detalle);
        orden.Total_orden = orden.DetallesOrden.Sum(d => d.SubTotal)
                          + orden.DetallesServicio.Sum(d => d.SubTotal);

        await _ordenRepo.ActualizarAsync(orden);
    }

    public async Task AgregarServicioAsync(AgregarServicioOrdenRequest req)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(req.IdOrden)
            ?? throw new EntidadNoEncontradaException("Orden", req.IdOrden);

        var detalle = new DetalleServicio
        {
            Id_orden            = req.IdOrden,
            Id_servicio         = req.IdServicio,
            Id_empleado_tecnico = req.IdEmpleadoTecnico,
            Descripcion_trabajo = req.DescripcionTrabajo,
            Precio_cobrado      = req.PrecioCobrado,
            SubTotal            = req.PrecioCobrado
        };

        orden.DetallesServicio.Add(detalle);
        orden.Total_orden = orden.DetallesOrden.Sum(d => d.SubTotal)
                          + orden.DetallesServicio.Sum(d => d.SubTotal);

        await _ordenRepo.ActualizarAsync(orden);
    }

    public async Task ConfirmarOrdenAsync(int idOrden)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(idOrden)
            ?? throw new EntidadNoEncontradaException("Orden", idOrden);

        if (!orden.PuedeConfirmarse())
            throw new OrdenNoConfirmableException(idOrden);

        foreach (var detalle in orden.DetallesOrden)
        {
            var producto = await _productoRepo.ObtenerPorIdAsync(detalle.Id_producto)
                ?? throw new EntidadNoEncontradaException("Producto", detalle.Id_producto);

            if (producto.Stock_actual < detalle.Cantidad)
                throw new StockInsuficienteException(producto.Descripcion_producto);

            producto.Stock_actual -= detalle.Cantidad;
            await _productoRepo.ActualizarAsync(producto);
        }

        orden.Estado_orden = "Confirmada";
        await _ordenRepo.ActualizarAsync(orden);
    }

    public async Task CancelarOrdenAsync(int idOrden)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(idOrden)
            ?? throw new EntidadNoEncontradaException("Orden", idOrden);

        if (!orden.PuedeCancelarse())
            throw new DomainException("No se puede cancelar una orden ya entregada.");

        if (orden.Estado_orden == "Confirmada")
        {
            foreach (var detalle in orden.DetallesOrden)
            {
                var producto = await _productoRepo.ObtenerPorIdAsync(detalle.Id_producto);
                if (producto != null)
                {
                    producto.Stock_actual += detalle.Cantidad;
                    await _productoRepo.ActualizarAsync(producto);
                }
            }
        }

        orden.Estado_orden = "Cancelada";
        await _ordenRepo.ActualizarAsync(orden);
    }

    public async Task<IEnumerable<Orden>> ObtenerCotizacionesAsync(int? idCliente = null)
        => await _ordenRepo.ObtenerCotizacionesAsync(idCliente);

    public async Task<IEnumerable<Orden>> ObtenerOrdenesAsync(string? estado = null, int? idCliente = null)
        => await _ordenRepo.ObtenerTodosAsync(estado, idCliente);
}
```

Crea el archivo ClienteUseCases.cs con este contenido:
```
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class ClienteUseCases
{
    private readonly IClienteRepository _repo;
    public ClienteUseCases(IClienteRepository repo) => _repo = repo;

    public async Task<Cliente> RegistrarClienteAsync(CrearClienteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NombreCliente))
            throw new DomainException("El nombre del cliente es requerido.");

        var cliente = new Cliente
        {
            Id_sector        = req.IdSector,
            Id_municipio     = req.IdMunicipio,
            Id_provincia     = req.IdProvincia,
            Nombre_cliente   = req.NombreCliente.Trim(),
            Apellido_cliente = req.ApellidoCliente.Trim(),
            RNC_cliente      = req.RNC,
            Cedula_cliente   = req.Cedula,
            Telefono_cliente = req.Telefono,
            Correo_cliente   = req.Correo,
            Fecha_registro   = DateTime.UtcNow
        };

        return await _repo.InsertarAsync(cliente);
    }

    public async Task<IEnumerable<Cliente>> BuscarAsync(string? buscar = null)
        => await _repo.ObtenerTodosAsync(buscar);

    public async Task<Cliente> ObtenerPorIdAsync(int id)
        => await _repo.ObtenerPorIdAsync(id)
            ?? throw new EntidadNoEncontradaException("Cliente", id);
}
```

Crea el archivo ProductoUseCases.cs con este contenido:
```
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class ProductoUseCases
{
    private readonly IProductoRepository _repo;
    public ProductoUseCases(IProductoRepository repo) => _repo = repo;

    public async Task<Producto> RegistrarProductoAsync(CrearProductoRequest req)
    {
        if (req.PrecioUnitario < req.CostoUnitario)
            throw new PrecioInvalidoException();

        var producto = new Producto
        {
            Id_categoria         = req.IdCategoria,
            Id_tipo_trans        = req.IdTipoTrans,
            Descripcion_producto = req.Descripcion,
            Precio_unitario      = req.PrecioUnitario,
            Costo_unitario       = req.CostoUnitario,
            Marca                = req.Marca,
            Stock_actual         = req.StockInicial,
            Activo               = true
        };

        return await _repo.InsertarAsync(producto);
    }

    public async Task ActualizarPrecioAsync(int idProducto, decimal nuevoPrecio, decimal nuevoCosto)
    {
        if (nuevoPrecio < nuevoCosto)
            throw new PrecioInvalidoException();

        var producto = await _repo.ObtenerPorIdAsync(idProducto)
            ?? throw new EntidadNoEncontradaException("Producto", idProducto);

        producto.Precio_unitario = nuevoPrecio;
        producto.Costo_unitario  = nuevoCosto;
        await _repo.ActualizarAsync(producto);
    }

    public async Task<IEnumerable<Producto>> ObtenerTodosAsync(int? idCategoria = null, bool soloConStock = false)
        => await _repo.ObtenerTodosAsync(idCategoria, soloConStock);
}
```

Crea el archivo CajaUseCases.cs con este contenido:
```
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class CajaUseCases
{
    private readonly ICajaRepository _repo;
    public CajaUseCases(ICajaRepository repo) => _repo = repo;

    public async Task AbrirCajaAsync(int idCaja, int idUsuarioApertura, decimal saldoInicial)
    {
        var caja = await _repo.ObtenerPorIdAsync(idCaja)
            ?? throw new EntidadNoEncontradaException("Caja", idCaja);

        if (caja.Estado == "Abierta")
            throw new CajaYaAbiertaException(caja.Codigo_caja);

        caja.Estado              = "Abierta";
        caja.Id_usuario_apertura = idUsuarioApertura;
        caja.Saldo_inicial       = saldoInicial;
        caja.Saldo_final         = saldoInicial;
        caja.Ultima_apertura     = DateTime.UtcNow;
        caja.Tipo_movimiento     = "Apertura";

        await _repo.ActualizarAsync(caja);
    }

    public async Task CerrarCajaAsync(int idCaja, int idUsuarioCierre)
    {
        var caja = await _repo.ObtenerPorIdAsync(idCaja)
            ?? throw new EntidadNoEncontradaException("Caja", idCaja);

        if (caja.Estado != "Abierta")
            throw new CajaCerradaException(caja.Codigo_caja);

        caja.Estado            = "Cerrada";
        caja.Id_usuario_cierre = idUsuarioCierre;
        caja.Ultimo_cierre     = DateTime.UtcNow;
        caja.Tipo_movimiento   = "Cierre";

        await _repo.ActualizarAsync(caja);
    }
}
```

Crea el archivo FacturaUseCases.cs con este contenido:
```
using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class FacturaUseCases
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IOrdenRepository _ordenRepo;
    private readonly ICajaRepository _cajaRepo;

    public FacturaUseCases(IFacturaRepository facturaRepo, IOrdenRepository ordenRepo, ICajaRepository cajaRepo)
    {
        _facturaRepo = facturaRepo;
        _ordenRepo   = ordenRepo;
        _cajaRepo    = cajaRepo;
    }

    public async Task<Factura> GenerarFacturaAsync(int idOrden, int idEmpleado, int idCaja, string tipoFactura = "Consumidor_Final")
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(idOrden)
            ?? throw new EntidadNoEncontradaException("Orden", idOrden);

        if (orden.Estado_orden != "Confirmada")
            throw new FacturaSinOrdenConfirmadaException();

        var caja = await _cajaRepo.ObtenerPorIdAsync(idCaja)
            ?? throw new EntidadNoEncontradaException("Caja", idCaja);

        if (caja.Estado != "Abierta")
            throw new CajaCerradaException(caja.Codigo_caja);

        var subtotal = orden.Total_orden ?? 0;
        var itbis    = subtotal * 0.18m;
        var total    = subtotal + itbis;
        var numero   = await _facturaRepo.GenerarNumeroFacturaAsync();

        var factura = new Factura
        {
            Id_orden       = idOrden,
            Id_cliente     = orden.Id_cliente,
            Id_empleado    = idEmpleado,
            Id_caja        = idCaja,
            Numero_factura = numero,
            Fecha_factura  = DateTime.UtcNow,
            Tipo_factura   = tipoFactura,
            SubTotal       = subtotal,
            ITBIS          = itbis,
            Total          = total,
            Estado         = "Emitida"
        };

        var facturaCreada = await _facturaRepo.InsertarAsync(factura);

        orden.Estado_orden = "Entregada";
        await _ordenRepo.ActualizarAsync(orden);

        caja.Saldo_final    += total;
        caja.Tipo_movimiento = "Venta";
        await _cajaRepo.ActualizarAsync(caja);

        return facturaCreada;
    }
}
```

=============================================================
PROYECTO 2: TransmisionesInfraestructura
=============================================================

CARPETA: TransmisionesInfraestructura/Data/

Crea el archivo TransmisionesContext.cs con el DbContext completo que incluya todos los DbSet para las 32 entidades del sistema de transmisiones, los mapeos de nombres de tabla con ToTable(), las claves compuestas para DetalleOrden, DetalleCompra y DetalleServicio, y las relaciones especiales de Caja (dos FK a Usuario: UsuarioApertura y UsuarioCierre con DeleteBehavior.Restrict) y DetalleServicio (FK a EmpleadoTecnico con DeleteBehavior.Restrict). Namespace: TransmisionesInfraestructura.Data

CARPETA: TransmisionesInfraestructura/Repositories/

Crea los repositorios ClienteRepository, ProductoRepository, OrdenRepository, FacturaRepository, CajaRepository, EmpleadoRepository, UsuarioRepository y VehiculoRepository implementando sus respectivas interfaces del Core. Cada repositorio debe usar TransmisionesContext con inyección de dependencias e incluir los Include() necesarios para cargar las relaciones. Namespace: TransmisionesInfraestructura.Repositories

=============================================================
PROYECTO 3: TransmisionesAPI
=============================================================

Reemplaza el Program.cs con la configuración completa que registre el DbContext con UseSqlServer leyendo el connection string "DefaultConnection" del appsettings.json, registre todos los repositorios y casos de uso con AddScoped, configure Swagger, CORS con AllowAll, y agregue un endpoint GET /test-db que retorne la cantidad de clientes y productos.

Crea los controllers ClientesController, ProductosController, OrdenesController, FacturasController y CajasController con sus endpoints REST completos usando los casos de uso del Core.

Reemplaza el appsettings.json con la estructura que tenga el ConnectionString de Azure SQL apuntando a transmisiones-server.database.windows.net con la base de datos TransmisionesDB. Deja el campo Password con el placeholder TU_PASSWORD_AQUI.

=============================================================
NOTAS FINALES PARA GEMINI:
=============================================================
- El proyecto usa .NET 8
- Todos los archivos deben compilar sin errores
- Usa async/await en todos los métodos de repositorio
- Los namespaces deben coincidir exactamente con los indicados
- No agregues nada extra que no se haya pedido
