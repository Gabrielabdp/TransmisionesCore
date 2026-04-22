using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly TransmisionesContext _context;
    public VehiculoRepository(TransmisionesContext context) => _context = context;

    public async Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula)
        => await _context.Vehiculos
            .Include(v => v.Cliente)
            .Include(v => v.TipoTransmision)
            .FirstOrDefaultAsync(v => v.Matricula == matricula);

    public async Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente)
        => await _context.Vehiculos
            .Where(v => v.Id_cliente == idCliente)
            .ToListAsync();

    public async Task<Vehiculo> InsertarAsync(Vehiculo vehiculo)
    {
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();
        return vehiculo;
    }

    public async Task EliminarAsync(string matricula)
    {
        var v = await _context.Vehiculos.FindAsync(matricula);
        if (v != null)
        {
            _context.Vehiculos.Remove(v);
            await _context.SaveChangesAsync();
        }
    }
}
