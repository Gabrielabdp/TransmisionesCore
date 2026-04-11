using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioUseCases _useCases;

    public UsuariosController(UsuarioUseCases useCases)
    {
        _useCases = useCases;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var usuario = await _useCases.LoginAsync(request.Email, request.Password);
        if (usuario == null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        return Ok(usuario);
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto request)
    {
        try
        {
            var usuario = await _useCases.RegistrarClienteAsync(request.Nombre, request.Apellido, request.Email, request.Password);
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
