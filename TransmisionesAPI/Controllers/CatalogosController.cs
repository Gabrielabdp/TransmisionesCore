using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data; // Asegúrate de que este sea el namespace de tu Context

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly TransmisionesContext _context;
    private readonly ProductoUseCases _productoUseCases;

        public CatalogosController(TransmisionesContext context, ProductoUseCases productoUseCases)
    {
        _context = context;
        _productoUseCases = productoUseCases;
    }

    [HttpGet("condiciones-pago")]
    public async Task<IActionResult> ObtenerCondicionesPago()
    {
        // Consultamos directo al contexto
        var lista = await _context.CondicionesPagos
            .Select(c => new {
                c.Id_condicion_pago,
                c.Descripcion,
                // Si el plazo es nulo, devolvemos 0 para evitar errores en el ComboBox
                Plazo = c.Plazo_pago ?? 0
            })
            .ToListAsync();

        return Ok(lista);
    }
    [HttpGet("provincias")]
    public async Task<IActionResult> GetProvincias()
    => Ok(await _context.Provincias.ToListAsync());

    [HttpGet("municipios/{idProvincia}")]
    public async Task<IActionResult> GetMunicipios(int idProvincia)
        => Ok(await _context.Municipios.Where(m => m.Id_provincia == idProvincia).ToListAsync());

    [HttpGet("comprobantes")]
    public async Task<IActionResult> GetComprobantes()
        => Ok(new[] {
        new { Id = 1, Nombre = "Consumo (B02)" },
        new { Id = 2, Nombre = "Crédito Fiscal (B01)" }
        });

    [HttpGet("bajo-stock")]
    public async Task<IActionResult> GetBajoStock()
    {
        var alertas = await _productoUseCases.ObtenerAlertasStockAsync();
        return Ok(alertas);
    }
}