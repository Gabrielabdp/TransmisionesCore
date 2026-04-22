using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesInfraestructura.Data;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly TransmisionesContext _context;

    public ServiciosController(TransmisionesContext context) => _context = context;

    // GET api/servicios
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var servicios = await _context.Servicios
            .Include(s => s.TipoServicio)
            .Select(s => new {
                s.Id_servicio,
                s.Nombre_servicio,
                s.Descripcion,
                s.Precio_base,
                s.Activo,
                s.Id_tipo_servicio,
                TipoServicio = s.TipoServicio.Descripcion
            })
            .ToListAsync();

        return Ok(servicios);
    }

    // POST api/servicios
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ServicioRequest req)
    {
        var servicio = new Servicio
        {
            Id_tipo_servicio = req.IdTipoServicio,
            Nombre_servicio = req.NombreServicio,
            Descripcion = req.Descripcion,
            Precio_base = req.PrecioBase,
            Activo = true
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();
        return Ok(servicio);
    }

    // PUT api/servicios/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ServicioRequest req)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio is null) return NotFound();

        servicio.Id_tipo_servicio = req.IdTipoServicio;
        servicio.Nombre_servicio = req.NombreServicio;
        servicio.Descripcion = req.Descripcion;
        servicio.Precio_base = req.PrecioBase;

        await _context.SaveChangesAsync();
        return Ok(servicio);
    }

    // PUT api/servicios/{id}/estado
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] ServicioEstadoRequest req)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio is null) return NotFound();

        servicio.Activo = req.Activo;
        await _context.SaveChangesAsync();
        return Ok(servicio);
    }
}

public record ServicioRequest(int IdTipoServicio, string NombreServicio, string? Descripcion, decimal PrecioBase);
public record ServicioEstadoRequest(bool Activo);