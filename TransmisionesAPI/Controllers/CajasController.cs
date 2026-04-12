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

    [HttpGet("resumen-hoy")]
    public async Task<IActionResult> GetResumenHoy()
    {
        try
        {
            var resumen = await _useCases.ObtenerResumenHoyAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            // Por si hay algún problema con la base de datos de Azure
            return StatusCode(500, new { mensaje = "Error al obtener el resumen", detalle = ex.Message });
        }

    }

    [HttpGet("{id}/estado-actual")]
    public async Task<ActionResult<EstadoCajaDTO>> GetEstadoActual(int id)
    {
        var resultado = await _useCases.ObtenerEstadoActualAsync(id);

        if (resultado == null)
        {
            return NotFound($"No se encontró la caja con ID {id}");
        }

        return Ok(resultado);
    }
}
