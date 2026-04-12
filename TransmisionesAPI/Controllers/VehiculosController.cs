using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.Entities;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers; 

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly VehiculoUseCases _useCases;

    public VehiculosController(VehiculoUseCases useCases) => _useCases = useCases;

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] Vehiculo vehiculo)
    {
        
        var nuevoVehiculo = await _useCases.RegistrarVehiculoAsync(vehiculo);
        return CreatedAtAction(nameof(ObtenerPorMatricula), new { matricula = nuevoVehiculo.Matricula }, nuevoVehiculo);
    }

    [HttpGet("matricula/{matricula}")]
    public async Task<IActionResult> ObtenerPorMatricula(string matricula)
    {
        var vehiculo = await _useCases.ObtenerPorMatriculaAsync(matricula);
        return vehiculo is not null ? Ok(vehiculo) : NotFound("Vehículo no encontrado.");
    }

    [HttpGet("cliente/{idCliente}")]
    public async Task<IActionResult> ObtenerPorCliente(int idCliente)
    {
        var vehiculos = await _useCases.ObtenerPorClienteAsync(idCliente);
        return Ok(vehiculos);

    }

    [HttpGet("{matricula}/historial")]
    public async Task<IActionResult> GetHistorial(string matricula)
    {
        var historial = await _useCases.ObtenerHistorial(matricula);
        if (historial == null) return NotFound(new { mensaje = "No hay historial para esta matrícula." });
        return Ok(historial);
    }
    [HttpGet("/api/garantias/verificar/{idVehiculo}")]
    public async Task<ActionResult<GarantiaEstadoDTO>> VerificarGarantia(int idVehiculo)
    {
        var resultado = await _useCases.VerificarGarantiaAsync(idVehiculo);
        return Ok(resultado);
    }

}