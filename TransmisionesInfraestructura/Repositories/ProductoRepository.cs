using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Entities;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;

namespace TransmisionesInfraestructura.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly TransmisionesContext _context;
    public ProductoRepository(TransmisionesContext context) => _context = context;

    public async Task<Producto?> ObtenerPorIdAsync(int id)
        => await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.TipoTransmision)
            .FirstOrDefaultAsync(p => p.Id_producto == id);

    public async Task<IEnumerable<Producto>> ObtenerTodosAsync(int? idCategoria = null, bool soloConStock = false)
    {
        var query = _context.Productos.AsQueryable();
        if (idCategoria.HasValue) query = query.Where(p => p.Id_categoria == idCategoria);
        if (soloConStock) query = query.Where(p => p.Stock_actual > 0);
        return await query.ToListAsync();
    }

    public async Task<Producto> InsertarAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task ActualizarAsync(Producto producto)
    {
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }

    public async Task<int> AjustarInventarioAsync(AjustarStockRequest request)
    {
        var resultado = await _context.Database
            .SqlQueryRaw<int>(
                "EXEC sp_AjustarInventario @Id_producto={0}, @Id_empleado={1}, @Tipo_ajuste={2}, @Cantidad={3}, @Motivo={4}",
                request.IdProducto, request.IdEmpleado, request.TipoAjuste, request.Cantidad, request.Motivo ?? (object)DBNull.Value
            )
            .ToListAsync();

        return resultado.FirstOrDefault();
    }
}
