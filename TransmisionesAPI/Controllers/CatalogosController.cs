using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesInfraestructura.Data; // Asegúrate de que este sea el namespace de tu Context

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly TransmisionesContext _context;

    // Inyectamos el contexto para poder consultar la base de datos de Azure
    public CatalogosController(TransmisionesContext context)
    {
        _context = context;
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
}