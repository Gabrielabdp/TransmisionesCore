using Microsoft.AspNetCore.Mvc;
using TransmisionesInfraestructura.Data;
using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SucursalesController : ControllerBase
{
    private readonly TransmisionesContext _context;
    public SucursalesController(TransmisionesContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _context.Sucursales.Where(s => s.Activa).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] SucursalRequest req)
    {
        var s = new Sucursal
        {
            Nombre_sucursal = req.NombreSucursal,
            Direccion = req.Direccion,
            Telefono = req.Telefono,
            Id_municipio = req.IdMunicipio,
            Activa = true
        };
        _context.Sucursales.Add(s);
        await _context.SaveChangesAsync();
        return Ok(s);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] SucursalRequest req)
    {
        var s = await _context.Sucursales.FindAsync(id);
        if (s == null) return NotFound();
        s.Nombre_sucursal = req.NombreSucursal;
        s.Direccion = req.Direccion;
        s.Telefono = req.Telefono;
        s.Id_municipio = req.IdMunicipio;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record SucursalRequest(string NombreSucursal, string? Direccion, string? Telefono, int IdMunicipio);