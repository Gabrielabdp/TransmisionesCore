using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesInfraestructura.Data;

namespace TransmisionesAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly TransmisionesContext _context;

    public ServiciosController(TransmisionesContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> ListarActivos()
    {
        // Traemos solo los que están activos para el ComboBox
        var servicios = await _context.Servicios
            .Where(s => s.Activo)
            .Select(s => new {
                s.Id_servicio,
                s.Nombre_servicio,
                s.Precio_base
            })
            .ToListAsync();

        return Ok(servicios);
    }
}