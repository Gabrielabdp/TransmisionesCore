using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/autenticacion")]
    [ApiController]
    public class IntegracionAutenticacionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IntegracionDbContext _context;

        public IntegracionAutenticacionController(HttpClient httpClient, IntegracionDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(5); // Timeout corto para detectar rápido la caída

                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var respuesta = await _httpClient.PostAsync("https://localhost:56678/api/Autenticacion/login", jsonContent);

                if (respuesta.IsSuccessStatusCode)
                {
                    var json = await respuesta.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }

                if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Unauthorized(new { mensaje = "Acceso denegado. Credenciales incorrectas." });
                }

                return await ValidarLoginOffline(request);
            }
            catch
            {
                // Si el servidor no responde (Exception/Timeout), caemos al modo offline
                return await ValidarLoginOffline(request);
            }
        }

        private async Task<IActionResult> ValidarLoginOffline(LoginRequest request)
        {
            var empleadoLocal = await _context.EmpleadosCache
                .FirstOrDefaultAsync(e => e.UsuarioAcceso == request.Usuario && e.Activo);

            if (empleadoLocal == null)
                return Unauthorized(new { mensaje = "(Offline) Usuario no encontrado o inactivo." });

           
            bool passwordValido = request.Password == empleadoLocal.PasswordHash;

            if (!passwordValido)
                return Unauthorized(new { mensaje = "(Offline) Credenciales incorrectas." });

            // Retornamos una estructura idéntica a la que devuelve el CORE para no quebrar el frontend
            return Ok(new
            {
                id_empleado = empleadoLocal.Id,
                nombre_usuario = empleadoLocal.UsuarioAcceso,
                rol = empleadoLocal.Rol,
                modoOffline = true,
                mensaje = "Has iniciado sesión en modo desconectado."
            });
        }
    }

    public record LoginRequest(string Usuario, string Password);
}
