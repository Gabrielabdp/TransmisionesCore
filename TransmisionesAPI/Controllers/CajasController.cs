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
    public async Task<IActionResult> Cerrar(int id, int idUsuario)
    {
        await _useCases.CerrarCajaAsync(id, idUsuario);
        return Ok();
    }
}
