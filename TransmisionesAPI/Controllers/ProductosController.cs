using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;
using TransmisionesCore.Entities;


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

    [HttpPost("ajustar-stock")]
    public async Task<IActionResult> AjustarStock([FromBody] AjustarStockRequest req)
    {
        try
        {
            // Llamamos al UseCase que conectamos al Repositorio
            var nuevoStock = await _useCases.AjustarDeInventarioAsync(req);

            return Ok(new
            {
                mensaje = "Inventario actualizado correctamente en Azure",
                stockActual = nuevoStock
            });
        }
        catch (Exception ex)
        {
            // Esto atrapará errores como "Stock insuficiente" enviados desde el SP
            return BadRequest(new { error = ex.Message });
        }

    }
}
