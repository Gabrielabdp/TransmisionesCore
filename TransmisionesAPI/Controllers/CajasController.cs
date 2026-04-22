using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CajasController : ControllerBase
{
    private readonly CajaUseCases _useCases;
    public CajasController(CajaUseCases useCases) => _useCases = useCases;

    [HttpPost("{id}/abrir")]
    public async Task<IActionResult> Abrir(int id, int idUsuario, decimal saldoInicial)
    {
        await _useCases.AbrirCajaAsync(id, idUsuario, saldoInicial);
        return Ok();
    }

    [HttpPost("{id}/cerrar")]
    public async Task<IActionResult> Cerrar(int id, int idUsuario, decimal saldoFinal)
    {
        await _useCases.CerrarCajaAsync(id, idUsuario, saldoFinal);
        return Ok();
    }

    [HttpGet("{id}/reporte")]
    public async Task<IActionResult> ObtenerReporte(int id)
    {
        return Ok(await _useCases.GenerarReporteCierreAsync(id));
    }

    [HttpPost("{id}/movimiento")]
    public async Task<IActionResult> RegistrarMovimiento(int id, [FromBody] RegistrarMovimientoDto req)
    {
        await _useCases.RegistrarMovimientoAsync(id, req.IdUsuario, req.Monto, req.Tipo, req.Motivo);
        return Ok();
    }
}
