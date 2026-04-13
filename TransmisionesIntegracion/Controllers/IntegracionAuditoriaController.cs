using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegracionAuditoriaController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionAuditoriaController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet("precios")]
        public async Task<IActionResult> ObtenerAuditoriaPrecios()
        {
            return await EjecutarConsultaProxy("https://localhost:56678/api/Auditoria/precios");
        }
        private async Task<IActionResult> EjecutarConsultaProxy(string urlCore)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(8); // Tiempo corto para no bloquear la UI
                var respuesta = await cliente.GetAsync(urlCore);

                if (respuesta.IsSuccessStatusCode)
                {
                    var json = await respuesta.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                return StatusCode((int)respuesta.StatusCode, "El servicio central reportó un error.");
            }
            catch (Exception)
            {
                return StatusCode(503, new
                {
                    error = "Offline",
                    mensaje = "Este reporte o consulta avanzada requiere conexión a internet y no está disponible en modo local."
                });
            }
        }
    }
}
