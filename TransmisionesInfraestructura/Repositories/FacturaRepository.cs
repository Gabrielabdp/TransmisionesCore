using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly TransmisionesContext _context;
    public FacturaRepository(TransmisionesContext context) => _context = context;

    public async Task<Factura?> ObtenerPorIdAsync(int id)
        => await _context.Facturas
            .Include(f => f.Orden)
            .Include(f => f.Cliente)
            .Include(f => f.Empleado)
            .Include(f => f.Caja)
            .FirstOrDefaultAsync(f => f.Id_factura == id);

    public async Task<IEnumerable<Factura>> ObtenerTodosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Facturas.AsQueryable();
        if (desde.HasValue) query = query.Where(f => f.Fecha_factura >= desde);
        if (hasta.HasValue) query = query.Where(f => f.Fecha_factura <= hasta);
        return await query.ToListAsync();
    }

    public async Task<Factura> InsertarAsync(Factura factura)
    {
        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();
        return factura;
    }

    public async Task<string> GenerarNumeroFacturaAsync()
    {
        var count = await _context.Facturas.CountAsync();
        return $"FAC-{DateTime.Now:yyyyMMdd}-{count + 1:D4}";
    }
}
