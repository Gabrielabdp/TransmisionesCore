using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class OrdenRepository : IOrdenRepository
{
    private readonly TransmisionesContext _context;
    public OrdenRepository(TransmisionesContext context) => _context = context;

    public async Task<Orden?> ObtenerPorIdAsync(int id)
        => await _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.Empleado)
            .Include(o => o.DetallesOrden).ThenInclude(d => d.Producto)
            .Include(o => o.DetallesServicio).ThenInclude(d => d.Servicio)
            .FirstOrDefaultAsync(o => o.Id_orden == id);

    public async Task<IEnumerable<Orden>> ObtenerTodosAsync(string? estado = null, int? idCliente = null)
    {
        var query = _context.Ordenes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(o => o.Estado_orden == estado);
        if (idCliente.HasValue) query = query.Where(o => o.Id_cliente == idCliente);
        return await query.ToListAsync();
    }
    public async Task<IEnumerable<Orden>> ObtenerCotizacionesAsync(int? idCliente = null)
    { 
        var query = _context.Ordenes.Where(o => o.Tipo_orden == "Cotizacion");

        if (idCliente.HasValue)
        {
            query = query.Where(o => o.Id_cliente == idCliente.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Orden> InsertarAsync(Orden orden)
    {
        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();
        return orden;
    }

    public async Task ActualizarAsync(Orden orden)
    {
        _context.Ordenes.Update(orden);
        await _context.SaveChangesAsync();
    }
}
