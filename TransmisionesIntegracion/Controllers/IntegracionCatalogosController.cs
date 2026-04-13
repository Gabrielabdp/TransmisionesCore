using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/catalogos")]
    [ApiController]
    public class IntegracionCatalogosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionCatalogosController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet("condiciones-pago")]
        public async Task<IActionResult> GetCondiciones() => await ProxyOrCache("api/Catalogos/condiciones-pago", () => _context.CondicionesPagoCache.ToListAsync());

        [HttpGet("provincias")]
        public async Task<IActionResult> GetProvincias() => await ProxyOrCache("api/Catalogos/provincias", () => _context.ProvinciasCache.ToListAsync());

        [HttpGet("municipios/{idProvincia}")]
        public async Task<IActionResult> GetMunicipios(int idProvincia) => await ProxyOrCache($"api/Catalogos/municipios/{idProvincia}", () => _context.MunicipiosCache.Where(m => m.IdProvincia == idProvincia).ToListAsync());

        [HttpGet("comprobantes")]
        public async Task<IActionResult> GetComprobantes()
        {
            // Los comprobantes son estáticos y quemados en el CORE, podemos devolverlos directamente
            return Ok(new[] {
                new { id = 1, nombre = "Consumo (B02)" },
                new { id = 2, nombre = "Crédito Fiscal (B01)" }
            });
        }

        [HttpGet("bajo-stock")]
        public async Task<IActionResult> ObtenerAlertasStock()
        {
            return await EjecutarConsultaProxy("https://localhost:56678/api/Catalogos/bajo-stock");
        }

        // Método auxiliar para no repetir la lógica del try-catch proxy en cada endpoint
        private async Task<IActionResult> ProxyOrCache<T>(string urlCore, Func<Task<List<T>>> cacheQuery)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var respuesta = await cliente.GetAsync($"https://localhost:56678/{urlCore}");

                if (respuesta.IsSuccessStatusCode)
                {
                    var json = await respuesta.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                return Ok(new { modoOffline = true, datos = await cacheQuery() });
            }
            catch
            {
                return Ok(new { modoOffline = true, datos = await cacheQuery() });
            }
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