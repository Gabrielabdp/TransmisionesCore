using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class CajaRepository : ICajaRepository
{
    private readonly TransmisionesContext _context;
    public CajaRepository(TransmisionesContext context) => _context = context;

    public async Task<Caja?> ObtenerPorIdAsync(int id)
        => await _context.Cajas
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(c => c.Id_caja == id);

    public async Task<IEnumerable<Caja>> ObtenerTodosAsync(int? idSucursal = null)
    {
        var query = _context.Cajas.AsQueryable();
        if (idSucursal.HasValue) query = query.Where(c => c.Id_sucursal == idSucursal);
        return await query.ToListAsync();
    }

    public async Task ActualizarAsync(Caja caja)
    {
        _context.Cajas.Update(caja);
        await _context.SaveChangesAsync();
    }

    public async Task RegistrarMovimientoAsync(MovimientoCaja movimiento)
    {
        _context.MovimientosCaja.Add(movimiento);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MovimientoCaja>> ObtenerMovimientosAsync(int idCaja, DateTime desde)
    {
        return await _context.MovimientosCaja
            .Where(m => m.Id_caja == idCaja && m.Fecha >= desde)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }
}
