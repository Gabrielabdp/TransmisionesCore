using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.Services;

namespace TransmisionesCore.UseCases;

public class FacturaUseCases
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly ICajaRepository _cajaRepo;
    private readonly IOrdenRepository _ordenRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly EmailService _emailService;
    private readonly PdfService _pdfService;

    public FacturaUseCases(
        IFacturaRepository facturaRepo, 
        ICajaRepository cajaRepo,
        IOrdenRepository ordenRepo,
        IProductoRepository productoRepo,
        EmailService emailService,
        PdfService pdfService)
    {
        _facturaRepo = facturaRepo;
        _cajaRepo = cajaRepo;
        _ordenRepo = ordenRepo;
        _productoRepo = productoRepo;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    public async Task<Factura> VentaRapidaAsync(VentaRapidaRequest req)
    {
        // 1. Crear la Orden Automáticamente
        var orden = new Orden
        {
            Id_cliente = req.IdCliente,
            Id_empleado = req.IdEmpleado,
            Id_canal = 1, // Punto de Venta (Mostrador)
            Tipo_orden = "Venta",
            Estado_orden = "Confirmada",
            Fecha_orden = DateTime.UtcNow,
            Total_orden = 0
        };

        var ordenGuardada = await _ordenRepo.InsertarAsync(orden);

        // 2. Agregar Detalles y Actualizar Stock
        decimal total = 0;
        foreach (var item in req.Items)
        {
            var producto = await _productoRepo.ObtenerPorIdAsync(item.IdProducto);
            if (producto != null)
            {
                var subtotal = producto.Precio_unitario * item.Cantidad;
                ordenGuardada.DetallesOrden.Add(new DetalleOrden
                {
                    Id_orden = ordenGuardada.Id_orden,
                    Id_producto = producto.Id_producto,
                    Cantidad = item.Cantidad,
                    Precio_unitario = producto.Precio_unitario,
                    SubTotal = subtotal
                });
                total += subtotal;

                // Descontamos del inventario real
                producto.Stock_actual -= item.Cantidad;
                await _productoRepo.ActualizarAsync(producto);
            }
        }

        ordenGuardada.Total_orden = total;
        await _ordenRepo.ActualizarAsync(ordenGuardada);

        // 3. Emitir Factura
        return await EmitirFacturaYNotificarAsync(
            ordenGuardada.Id_orden, 
            req.IdCliente, 
            req.IdCaja, 
            req.EmailNotificacion ?? "cliente_final@mag.com", 
            total * 1.18m // Incluimos ITBIS
        );
    }

    public async Task<Factura> EmitirFacturaYNotificarAsync(int idOrden, int idCliente, int idCaja, string emailDestino, decimal montoTotal)
    {
        // 1. Generamos el número de factura oficial
        var numeroFactura = await _facturaRepo.GenerarNumeroFacturaAsync();

        // 2. Creamos el objeto factura
        var factura = new Factura
        {
            Id_orden = idOrden,
            Id_cliente = idCliente,
            Id_caja = idCaja,
            Id_empleado = 1, // Por defecto, asignamos al administrador del sistema
            Numero_factura = numeroFactura,
            Fecha_factura = DateTime.UtcNow,
            Tipo_factura = "Consumidor Final",
            Total = montoTotal,
            SubTotal = montoTotal / 1.18m,
            ITBIS = montoTotal - (montoTotal / 1.18m),
            Estado = "Pagada"
        };

        // 3. Persistencia Real en Base de Datos SQL Server
        var facturaGuardada = await _facturaRepo.InsertarAsync(factura);

        // 4. Actualización del Saldo de Caja (Entrada de dinero)
        var caja = await _cajaRepo.ObtenerPorIdAsync(idCaja);
        if (caja != null)
        {
            caja.Saldo_final += montoTotal;
            caja.Tipo_movimiento = "Venta";
            await _cajaRepo.ActualizarAsync(caja);
            
            // Registramos el movimiento contable detallado
            await _cajaRepo.RegistrarMovimientoAsync(new MovimientoCaja
            {
                Id_caja = idCaja,
                Id_usuario = 1, // Sistema
                Monto = montoTotal,
                Tipo = "Entrada",
                Motivo = $"Venta Terminal: {numeroFactura}",
                Fecha = DateTime.UtcNow
            });
        }

        // 5. ENVÍO REAL DE FACTURA PDF POR CORREO
        try 
        {
            var pdfData = await _pdfService.GenerarFacturaPdfAsync(facturaGuardada, "Cliente Transmisiones MAG");
            await _emailService.EnviarFacturaRealAsync(emailDestino, numeroFactura, pdfData);
        }
        catch (Exception ex)
        {
            // Logueamos el error pero no interrumpimos la venta
            Console.WriteLine($"[AVISO] No se pudo enviar el correo: {ex.Message}");
        }

        return facturaGuardada;
    }
}
