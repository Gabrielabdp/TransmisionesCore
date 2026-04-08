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

    public async Task RegistrarMovimientoAsync(int idCaja, int idUsuario, decimal monto, string tipo, string motivo)
    {
        var caja = await _repo.ObtenerPorIdAsync(idCaja)
            ?? throw new EntidadNoEncontradaException("Caja", idCaja);

        if (caja.Estado != "Abierta")
            throw new CajaCerradaException(caja.Codigo_caja);

        // Actualizamos el saldo
        if (tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
            caja.Saldo_final += monto;
        else if (tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
            caja.Saldo_final -= monto;

        caja.Tipo_movimiento = tipo;

        // Aquí el repositorio debería guardar el movimiento. 
        // Como el repo actual solo tiene 'ActualizarAsync' para la Caja, 
        // vamos a ampliar la interfaz del repositorio primero.
        await _repo.RegistrarMovimientoAsync(new MovimientoCaja
        {
            Id_caja = idCaja,
            Id_usuario = idUsuario,
            Monto = monto,
            Tipo = tipo,
            Motivo = motivo,
            Fecha = DateTime.UtcNow
        });

        await _repo.ActualizarAsync(caja);
    }
}
