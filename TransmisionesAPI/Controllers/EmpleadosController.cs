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

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? idSucursal)
    {
        var empleados = await _empleadoRepo.ObtenerTodosAsync(idSucursal);
        return Ok(empleados.Select(e => new
        {
            e.Id_empleado,
            e.Nombre,
            e.Apellido,
            NombreCompleto = $"{e.Nombre} {e.Apellido}",
            e.Cedula,
            e.Telefono,
            e.Email,
            e.Id_usuario,
            Rol = e.Usuario?.Rol ?? "",        // ✅ mapea el rol
            e.Id_sucursal,
            Sucursal = e.Sucursal == null ? null : new
            {
                e.Sucursal.Id_sucursal,
                e.Sucursal.Nombre_sucursal
            },
            e.Activo
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var e = await _empleadoRepo.ObtenerPorIdAsync(id);
        if (e == null) return NotFound("Empleado no encontrado");
        return Ok(new
        {
            e.Id_empleado,
            e.Nombre,
            e.Apellido,
            NombreCompleto = $"{e.Nombre} {e.Apellido}",
            e.Cedula,
            e.Telefono,
            e.Email,
            e.Id_usuario,
            Rol = e.Usuario?.Rol ?? "",
            e.Id_sucursal,
            Sucursal = e.Sucursal == null ? null : new
            {
                e.Sucursal.Id_sucursal,
                e.Sucursal.Nombre_sucursal
            },
            e.Activo
        });
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] EmpleadoRequest req)
    {
        try
        {
            var empleado = new Empleado
            {
                Id_usuario = req.IdUsuario,
                Id_sucursal = req.IdSucursal,
                Cedula = req.Cedula,
                Nombre = req.Nombre,
                Apellido = req.Apellido,
                Telefono = req.Telefono,
                Email = req.Email,
                Fecha_ingreso = req.FechaIngreso,
                Activo = true
            };
            var nuevo = await _empleadoRepo.InsertarAsync(empleado);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevo.Id_empleado }, nuevo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "Error al crear empleado", detalle = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] EmpleadoUpdateRequest req)
    {
        var empleado = await _empleadoRepo.ObtenerPorIdAsync(id);
        if (empleado == null) return NotFound("Empleado no encontrado");

        empleado.Nombre = req.Nombre;
        empleado.Apellido = req.Apellido;
        empleado.Telefono = req.Telefono;
        empleado.Email = req.Email;
        empleado.Id_sucursal = req.IdSucursal;

        await _empleadoRepo.ActualizarAsync(empleado);
        return NoContent();
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoRequest req)
    {
        var empleado = await _empleadoRepo.ObtenerPorIdAsync(id);
        if (empleado == null) return NotFound();
        empleado.Activo = req.Activo;
        await _empleadoRepo.ActualizarAsync(empleado);
        return NoContent();
    }

    public record EstadoRequest(bool Activo);
}



public record EmpleadoRequest(
    int IdUsuario,
    int IdSucursal,
    string Cedula,
    string Nombre,
    string Apellido,
    string? Telefono,
    string? Email,
    DateTime FechaIngreso
);

public record EmpleadoUpdateRequest(
    string Nombre,
    string Apellido,
    string? Telefono,
    string? Email,
    int IdSucursal
);
