using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
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

    public async Task<decimal> ObtenerVentasDelDiaAsync(DateTime fecha)
    {
        return await _context.Facturas
            .Where(f => f.Fecha_factura.Date == fecha.Date)
            .SumAsync(f => f.Total ?? 0); 
    }

    public async Task<EstadoCajaDTO> ObtenerEstadoActualAsync(int id)
    {
        var arqueo = await _context.Cajas
            .Where(c => c.Id_caja == id)
            .Select(c => new EstadoCajaDTO
            {
                Id_caja = c.Id_caja,
                Codigo_caja = c.Codigo_caja,
                Estado = c.Estado,
                Saldo_inicial = c.Saldo_inicial,

                // Sumamos los Cobros registrados a esta caja
                Total_Entradas = _context.Cobros
                    .Where(cobro => cobro.Id_caja == id)
                    .Sum(cobro => (decimal?)cobro.Monto_pago) ?? 0,

                // Sumamos los Pagos registrados a esta caja
                Total_Salidas = _context.Pagos
                    .Where(pago => pago.Id_caja == id)
                    .Sum(pago => (decimal?)pago.Monto_pago) ?? 0
            })
            .FirstOrDefaultAsync();

        if (arqueo != null)
        {
            arqueo.Balance_Calculado = arqueo.Saldo_inicial + arqueo.Total_Entradas - arqueo.Total_Salidas;
        }

        return arqueo;
    }
}
