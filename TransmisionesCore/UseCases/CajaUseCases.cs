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
