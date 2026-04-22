using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly TransmisionesContext _context;

    public UsuariosController(IUsuarioRepository usuarioRepo, TransmisionesContext context)
    {
        _usuarioRepo = usuarioRepo;
        _context = context;
    }

    // GET api/usuarios
    // GET api/usuarios?soloDisponibles=true  (sin empleado asignado)
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] bool soloDisponibles = false)
    {
        var usuarios = await _usuarioRepo.ObtenerTodosAsync();

        if (soloDisponibles)
        {
            var usadosIds = _context.Empleados
                .Select(e => e.Id_usuario)
                .ToHashSet();
            usuarios = usuarios.Where(u => !usadosIds.Contains(u.Id_usuario));
        }

        return Ok(usuarios.Select(u => new
        {
            u.Id_usuario,
            u.Nombre_usuario,
            u.Rol,
            u.Activo
        }));
    }

    // POST api/usuarios
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioCrearRequest req)
    {
        // Verificar que el nombre de usuario no esté en uso
        var existe = (await _usuarioRepo.ObtenerTodosAsync())
            .Any(u => u.Nombre_usuario == req.Nombre_usuario);
        if (existe)
            return BadRequest(new { mensaje = "Ese nombre de usuario ya está en uso." });

        var usuario = new Usuario
        {
            Nombre_usuario = req.Nombre_usuario,
            Contrasena = req.Contrasena,
            Rol = req.Rol,
            Activo = true
        };

        var nuevo = await _usuarioRepo.InsertarAsync(usuario);
        return Ok(new { nuevo.Id_usuario, nuevo.Nombre_usuario, nuevo.Rol, nuevo.Activo });
    }

    // PUT api/usuarios/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioActualizarRequest req)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

        // Verificar nombre duplicado (excluyendo el mismo)
        var duplicado = (await _usuarioRepo.ObtenerTodosAsync())
            .Any(u => u.Nombre_usuario == req.Nombre_usuario && u.Id_usuario != id);
        if (duplicado)
            return BadRequest(new { mensaje = "Ese nombre de usuario ya está en uso." });

        usuario.Nombre_usuario = req.Nombre_usuario;
        usuario.Rol = req.Rol;
        usuario.Activo = req.Activo;

        if (!string.IsNullOrWhiteSpace(req.Contrasena))
            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(req.Contrasena);

        await _usuarioRepo.ActualizarAsync(usuario);
        return NoContent();
    }

    // DELETE api/usuarios/{id}  →  soft delete (desactiva)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

        var empleadoVinculado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id_usuario == id);

        if (empleadoVinculado != null && empleadoVinculado.Activo)
            return BadRequest(new { mensaje = $"Tiene el empleado '{empleadoVinculado.Nombre} {empleadoVinculado.Apellido}' vinculado activo." });

        usuario.Activo = false;
        await _usuarioRepo.ActualizarAsync(usuario);
        return NoContent();
    }

}

public record UsuarioCrearRequest(string Nombre_usuario, string Contrasena, string Rol);
public record UsuarioActualizarRequest(string Nombre_usuario, string Rol, bool Activo, string? Contrasena);