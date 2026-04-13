using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteUseCases _useCases;
    public ClientesController(ClienteUseCases useCases) => _useCases = useCases;

    [HttpPost]
    public async Task<IActionResult> Registrar(CrearClienteRequest req)
        => Ok(await _useCases.RegistrarClienteAsync(req));

    [HttpGet]
    public async Task<IActionResult> Buscar(string? buscar)
        => Ok(await _useCases.BuscarAsync(buscar));

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
        => Ok(await _useCases.ObtenerPorIdAsync(id));

    [HttpGet("buscar/{documento}")]
    public async Task<IActionResult> BuscarPorDocumento(string documento)
    {
        
        var cliente = await _useCases.BuscarPorDocumento(documento);

        if (cliente == null)
        {
            return NotFound(new { mensaje = "No se encontró un cliente con ese documento." });
        }

        return Ok(cliente);
    }

    [HttpGet("{id}/resumen")]
    public async Task<IActionResult> ObtenerResumen(int id)
    {
        // Llamamos al UseCase que ya tiene la lógica de los Sum y Count
        var resumen = await _useCases.ObtenerResumenAsync(id);

        if (resumen == null)
        {
            return NotFound(new
            {
                mensaje = $"No se encontró información para el cliente con ID {id}."
            });
        }

        return Ok(resumen);
    }

    [HttpGet("{id}/resumen-vehiculos")]
    public async Task<ActionResult<ResumenFlotaDTO>> GetResumenVehiculos(int id)
    {
        var resumen = await _useCases.ObtenerResumenFlotaAsync(id);
        if (resumen == null) return NotFound();

        return Ok(resumen);
    }
}
