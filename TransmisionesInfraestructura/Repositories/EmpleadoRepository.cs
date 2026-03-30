using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly TransmisionesContext _context;
    public EmpleadoRepository(TransmisionesContext context) => _context = context;

    public async Task<Empleado?> ObtenerPorIdAsync(int id)
        => await _context.Empleados
            .Include(e => e.Usuario)
            .Include(e => e.Sucursal)
            .FirstOrDefaultAsync(e => e.Id_empleado == id);

    public async Task<IEnumerable<Empleado>> ObtenerTodosAsync(int? idSucursal = null)
    {
        var query = _context.Empleados.AsQueryable();
        if (idSucursal.HasValue) query = query.Where(e => e.Id_sucursal == idSucursal);
        return await query.ToListAsync();
    }

    public async Task<Empleado> InsertarAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
        return empleado;
    }

    public async Task ActualizarAsync(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
        await _context.SaveChangesAsync();
    }
}
