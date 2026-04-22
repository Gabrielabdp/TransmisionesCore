using Microsoft.AspNetCore.Mvc;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers;

[ApiController]
[Route("api/integracion/estado")]
public class IntegracionEstadoController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IntegracionDbContext _db;

    public IntegracionEstadoController(IHttpClientFactory httpFactory, IntegracionDbContext db)
    {
        _httpFactory = httpFactory;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstado()
    {
        bool azureDisponible = false;
        try
        {
            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(3);
            var resp = await http.GetAsync("https://localhost:56678/api/Cajas/resumen-hoy");
            azureDisponible = resp.IsSuccessStatusCode;
        }
        catch { /* Azure no alcanzable */ }

        int pendientes = _db.TransaccionesPendientes.Count(t => !t.Sincronizado);

        return Ok(new
        {
            azureDisponible,
            transaccionesPendientes = pendientes,
            timestamp = DateTime.UtcNow
        });
    }
}
