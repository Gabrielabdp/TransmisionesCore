using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly OrdenUseCases _useCases;
    public OrdenesController(OrdenUseCases useCases) => _useCases = useCases;

    [HttpPost]
    public async Task<IActionResult> Crear(CrearOrdenRequest req)
        => Ok(await _useCases.CrearOrdenAsync(req));

    [HttpPost("producto")]
    public async Task<IActionResult> AgregarProducto(AgregarProductoOrdenRequest req)
    {
        await _useCases.AgregarProductoAsync(req);
        return Ok();
    }

    [HttpPost("servicio")]
    public async Task<IActionResult> AgregarServicio(AgregarServicioOrdenRequest req)
    {
        await _useCases.AgregarServicioAsync(req);
        return Ok();
    }

    [HttpPost("{id}/confirmar")]
    public async Task<IActionResult> Confirmar(int id)
    {
        await _useCases.ConfirmarOrdenAsync(id);
        return Ok();
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _useCases.CancelarOrdenAsync(id);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerOrdenes(string? estado, int? idCliente)
        => Ok(await _useCases.ObtenerOrdenesAsync(estado, idCliente));

    [HttpGet("cotizaciones")]
    public async Task<IActionResult> ObtenerCotizaciones(int? idCliente)
        => Ok(await _useCases.ObtenerCotizacionesAsync(idCliente));
}
