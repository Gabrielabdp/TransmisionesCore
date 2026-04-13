using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;
using static TransmisionesCore.UseCases.ClienteUseCases;

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

    public async Task<Cliente?> ObtenerPorDocumentoAsync(string documento)
    {
        var busquedaSinGuiones = documento.Replace("-", "");

        return await _context.Clientes
            .FirstOrDefaultAsync(c =>
                (c.Cedula_cliente != null && c.Cedula_cliente.Replace("-", "") == busquedaSinGuiones) ||
                (c.RNC_cliente != null && c.RNC_cliente.Replace("-", "") == busquedaSinGuiones)
            );
    }

    public async Task<ClienteResumenDTO?> ObtenerResumenAsync(int id)
    {
        return await _context.Clientes
            .Where(c => c.Id_cliente == id)
            .Select(c => new ClienteResumenDTO(
                c.Id_cliente,
                $"{c.Nombre_cliente} {c.Apellido_cliente}",
                
                _context.Facturas.Where(f => f.Id_cliente == id && f.Estado == "Pendiente").Sum(f => (decimal?)f.Total) ?? 0,

                _context.Vehiculos.Count(v => v.Id_cliente == id),

                _context.Ordenes.Count(o => o.Id_cliente == id && o.Estado_orden != "Completada"),

                _context.Ordenes.Where(o => o.Id_cliente == id).Max(o => (DateTime?)o.Fecha_orden)
            ))
            .FirstOrDefaultAsync();
    }

}
