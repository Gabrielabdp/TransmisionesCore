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
}
