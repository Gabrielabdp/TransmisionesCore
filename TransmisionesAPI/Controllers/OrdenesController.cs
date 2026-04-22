using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.Exceptions;
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


    [HttpPost("{id}/aprobar")]
       public async Task<IActionResult> Aprobar(int id)
    {
        try
        {
            // Llamamos al UseCase que ya tiene la lógica de validación
            var exito = await _useCases.AprobarCotizacionAsync(id);

            if (!exito)
                return NotFound(new { mensaje = $"No se encontró la orden con ID {id}" });

            return Ok(new { mensaje = "Cotización aprobada correctamente. El estado ha cambiado y el técnico puede proceder." });
        }
        catch (OrdenNoConfirmableException ex)
        {
            
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
        }


    }

    [HttpPatch("{id}/asignar-empleado")]
    public async Task<IActionResult> AsignarEmpleado(int id, [FromBody] AsignarEmpleadoRequest req)
    {
        try
        {
            var exito = await _useCases.AsignarEmpleadoAsync(id, req.IdEmpleado);

            if (!exito)
                return NotFound(new { mensaje = $"No se encontró la orden con ID {id}" });

            return Ok(new { mensaje = $"Empleado #{req.IdEmpleado} asignado exitosamente a la orden #{id}." });
        }
        catch (EntidadNoEncontradaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }


    }
    [HttpPost("{id}/convertir")]
    public async Task<IActionResult> ConvertirAFactura(int id)
    {
        try
        {
            var factura = await _useCases.ConvertirAFacturaAsync(id);
            return Ok(new
            {
                mensaje = "Orden convertida a factura exitosamente",
                facturaId = factura.Id_factura,
                numero = factura.Numero_factura
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
    [HttpPost("{id}/anular")]
    public async Task<IActionResult> AnularOrden(int id)
    {
        try
        {
            await _useCases.AnularOrdenConReversionAsync(id);
            return Ok(new { mensaje = "Orden y facturas asociadas anuladas correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "No se pudo anular la orden.", detalle = ex.Message });
        }
    }

}
