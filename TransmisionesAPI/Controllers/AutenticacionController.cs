using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacionController : ControllerBase
{
    private readonly AutenticacionUseCase _autenticacionUseCase;

    // Inyectamos el caso de uso con el nuevo nombre
    public AutenticacionController(AutenticacionUseCase autenticacionUseCase)
    {
        _autenticacionUseCase = autenticacionUseCase;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Llamamos al método de validación
        var resultado = await _autenticacionUseCase.ValidarAccesoAsync(request.Usuario, request.Password);

        if (resultado == null)
        {
            return Unauthorized(new
            {
                mensaje = "Acceso denegado. El usuario o la contraseña son incorrectos."
            });
        }

        // Si es correcto, devuelve el objeto con Id_empleado, Rol, etc.
        return Ok(resultado);
    }
}

// Estructura para recibir los datos desde el Swagger o el App
public record LoginRequest(string Usuario, string Password);