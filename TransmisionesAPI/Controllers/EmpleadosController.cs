using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoRepository _empleadoRepo;

    public EmpleadosController(IEmpleadoRepository empleadoRepo)
    {
        _empleadoRepo = empleadoRepo;
    }

    // Listar empleados (opcionalmente por sucursal)
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? idSucursal)
    {
        var empleados = await _empleadoRepo.ObtenerTodosAsync(idSucursal);
        return Ok(empleados);
    }

    // Buscar un empleado específico por su ID
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var empleado = await _empleadoRepo.ObtenerPorIdAsync(id);
        return empleado != null ? Ok(empleado) : NotFound("Empleado no encontrado");
    }

    // Registrar un nuevo empleado
    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] Empleado empleado)
    {
        try
        {
            var nuevoEmpleado = await _empleadoRepo.InsertarAsync(empleado);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoEmpleado.Id_empleado }, nuevoEmpleado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "Error al crear empleado", detalle = ex.Message });
        }
    }
}