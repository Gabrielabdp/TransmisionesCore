using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class OrdenUseCases
{
    private readonly IOrdenRepository _ordenRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IEmpleadoRepository _empleadoRepo;
    private readonly IFacturaRepository _facturaRepo;

    public OrdenUseCases(IOrdenRepository ordenRepo, IProductoRepository productoRepo, IClienteRepository clienteRepo, IEmpleadoRepository empleadoRepo, IFacturaRepository facturaRepo)
    {
        _ordenRepo    = ordenRepo;
        _productoRepo = productoRepo;
        _clienteRepo  = clienteRepo;
        _empleadoRepo = empleadoRepo;
        _facturaRepo = facturaRepo;
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

    // ... otros métodos (Crear, AgregarProducto, etc) ...

    public async Task<bool> AprobarCotizacionAsync(int id)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(id);

        if (orden == null) return false;

        // Validación de tu entidad
        if (!orden.PuedeConfirmarse())
        {
            throw new OrdenNoConfirmableException(id);
        }

        // Cambio de estado
        orden.Estado_orden = "Aprobada";

        // Aquí aplicamos el ajuste:
        await _ordenRepo.ActualizarAsync(orden);
        return true;
    }

    public async Task<bool> AsignarEmpleadoAsync(int idOrden, int idEmpleado)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(idOrden);
        if (orden == null) return false;

        var empleado = await _empleadoRepo.ObtenerPorIdAsync(idEmpleado);
        if (empleado == null)
            throw new EntidadNoEncontradaException("Empleado", idEmpleado);

        orden.Id_empleado = idEmpleado;

        await _ordenRepo.ActualizarAsync(orden);
        return true;
    }

    public async Task<Factura> ConvertirAFacturaAsync(int idOrden)
    {
        var orden = await _ordenRepo.ObtenerPorIdAsync(idOrden);
        if (orden == null) throw new EntidadNoEncontradaException("Orden", idOrden);


        if (orden.Estado_orden == "Facturada")
            throw new DomainException("Esta orden ya ha sido convertida a factura.");

        var nuevaFactura = new Factura
        {
            Id_orden = orden.Id_orden,
            Id_cliente = orden.Id_cliente,
            Id_empleado = orden.Id_empleado,
            Numero_factura = $"FAC-{DateTime.Now}",
            Fecha_factura = DateTime.UtcNow,
            SubTotal = orden.Total_orden,
            ITBIS = (orden.Total_orden ?? 0) * 0.18m,
            Total = (orden.Total_orden ?? 0) * 1.18m,
            Estado = "Emitida"
        };

        orden.Estado_orden = "Facturada";
        await _ordenRepo.ActualizarAsync(orden);

       return await _facturaRepo.InsertarAsync(nuevaFactura);
    }

    public async Task<bool> AnularOrdenConReversionAsync(int id)
    {
       
        var orden = await _ordenRepo.ObtenerPorIdAsync(id);
        if (orden == null) throw new EntidadNoEncontradaException("Orden", id);

      
        orden.Estado_orden = "Anulada";
        await _ordenRepo.ActualizarAsync(orden);

        var facturas = await _facturaRepo.ObtenerTodosAsync();
        var facturaAsociada = facturas.FirstOrDefault(f => f.Id_orden == id);

        if (facturaAsociada != null)
        {
            facturaAsociada.Estado = "Anulada";
         
            await _facturaRepo.ActualizarAsync(facturaAsociada);
        }

        return true;
    }

}
