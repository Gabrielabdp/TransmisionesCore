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
