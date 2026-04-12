using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
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

    public async Task<HistorialVehiculoDTO?> ObtenerHistorialPorMatriculaAsync(string matricula)
    {
        return await _context.Vehiculos
            .Include(v => v.Cliente)
            .Where(v => v.Matricula == matricula)
            .Select(v => new HistorialVehiculoDTO(
                v.Matricula,
                $"{v.Marca} {v.Modelo} ({v.Ano})",
                $"{v.Cliente.Nombre_cliente} {v.Cliente.Apellido_cliente}",

                _context.Ordenes
                    .Where(o => o.Id_vehiculo == matricula) 
                    .OrderByDescending(o => o.Fecha_orden)  
                    .Select(o => new ServicioHistorialDTO(
                        o.Id_orden,
                        o.Fecha_orden,
                        o.Tipo_orden, 
                        "Servicio de Transmisión", 
                        o.Total_orden ?? 0,
                        o.Estado_orden
                    )).ToList()
            ))
            .FirstOrDefaultAsync();
    }

}
