using System;
using System.Threading.Tasks;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.Services;

namespace TransmisionesCore.UseCases;

public class FacturaUseCases
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly ICajaRepository _cajaRepo;
    private readonly EmailService _emailService;
    private readonly PdfService _pdfService;

    public FacturaUseCases(
        IFacturaRepository facturaRepo, 
        ICajaRepository cajaRepo,
        EmailService emailService,
        PdfService pdfService)
    {
        _facturaRepo = facturaRepo;
        _cajaRepo = cajaRepo;
        _emailService = emailService;
        _pdfService = pdfService;
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
                Motivo = $"Venta Web: {numeroFactura}",
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
