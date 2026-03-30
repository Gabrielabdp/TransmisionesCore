using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly TransmisionesContext _context;
    public ClienteRepository(TransmisionesContext context) => _context = context;

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
        => await _context.Clientes
            .Include(c => c.Sector)
            .Include(c => c.Municipio)
            .Include(c => c.Provincia)
            .FirstOrDefaultAsync(c => c.Id_cliente == id);

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(string? buscar = null)
    {
        var query = _context.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(c => c.Nombre_cliente.Contains(buscar) || 
                                     c.Apellido_cliente.Contains(buscar) || 
                                     c.Cedula_cliente.Contains(buscar));
        }
        return await query.ToListAsync();
    }

    public async Task<Cliente> InsertarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }
}
