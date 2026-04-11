using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaUseCases _useCases;

    public FacturasController(FacturaUseCases useCases)
    {
        _useCases = useCases;
    }

    [HttpPost("emitir")]
    public async Task<IActionResult> Emitir([FromBody] EmitirFacturaRequest request)
    {
        try
        {
            var factura = await _useCases.EmitirFacturaYNotificarAsync(
                request.IdOrden, 
                request.IdCliente, 
                request.IdCaja, 
                request.Email, 
                request.MontoTotal
            );
            return Ok(factura);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    public class EmitirFacturaRequest
    {
        public int IdOrden { get; set; }
        public int IdCliente { get; set; }
        public int IdCaja { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
    }
}
