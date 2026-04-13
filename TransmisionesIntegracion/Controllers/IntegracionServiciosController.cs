using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/servicios")]
    [ApiController]
    public class IntegracionServiciosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionServiciosController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerServicios()
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var respuesta = await cliente.GetAsync("https://localhost:56678/api/Servicios");

                if (respuesta.IsSuccessStatusCode)
                {
                    var json = await respuesta.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                return await ResponderDesdeCache();
            }
            catch
            {
                return await ResponderDesdeCache();
            }
        }

        private async Task<IActionResult> ResponderDesdeCache()
        {
            var locales = await _context.ServiciosCache.ToListAsync();
            return Ok(new { modoOffline = true, datos = locales });
        }
    }
}
