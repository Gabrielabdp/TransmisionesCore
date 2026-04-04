using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class ServicioRepository : IServicioRepository
{
    private readonly TransmisionesContext _context;
    public ServicioRepository(TransmisionesContext context) => _context = context;

    public async Task<Servicio?> ObtenerPorIdAsync(int id)
        => await _context.Servicios.FindAsync(id);

    public async Task<IEnumerable<Servicio>> ObtenerTodosAsync(bool soloActivos = true)
    {
        var query = _context.Servicios.AsQueryable();
        if (soloActivos) query = query.Where(s => s.Activo);
        return await query.ToListAsync();
    }

    public async Task<Servicio> InsertarAsync(Servicio servicio)
    {
        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();
        return servicio;
    }

    public async Task ActualizarAsync(Servicio servicio)
    {
        _context.Servicios.Update(servicio);
        await _context.SaveChangesAsync();
    }
}