using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ProductoUseCases _useCases;
    public ProductosController(ProductoUseCases useCases) => _useCases = useCases;

    [HttpPost]
    public async Task<IActionResult> Registrar(CrearProductoRequest req)
        => Ok(await _useCases.RegistrarProductoAsync(req));

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(int? idCategoria, bool soloConStock = false)
        => Ok(await _useCases.ObtenerTodosAsync(idCategoria, soloConStock));

    [HttpPatch("{id}/precio")]
    public async Task<IActionResult> ActualizarPrecio(int id, decimal nuevoPrecio, decimal nuevoCosto)
    {
        await _useCases.ActualizarPrecioAsync(id, nuevoPrecio, nuevoCosto);
        return NoContent();
    }
}
