using TransmisionesCore.Entities;
using TransmisionesCore.Exceptions;
using TransmisionesCore.Interfaces;

namespace TransmisionesCore.UseCases;

public class ProductoUseCases
{
    private readonly IProductoRepository _repo;
    public ProductoUseCases(IProductoRepository repo) => _repo = repo;

    public async Task<Producto> RegistrarProductoAsync(CrearProductoRequest req)
    {
        if (req.PrecioUnitario < req.CostoUnitario)
            throw new PrecioInvalidoException();

        var producto = new Producto
        {
            Id_categoria         = req.IdCategoria,
            Id_tipo_trans        = req.IdTipoTrans,
            Descripcion_producto = req.Descripcion,
            Precio_unitario      = req.PrecioUnitario,
            Costo_unitario       = req.CostoUnitario,
            Marca                = req.Marca,
            Stock_actual         = req.StockInicial,
            Activo               = true
        };

        return await _repo.InsertarAsync(producto);
    }

    public async Task ActualizarPrecioAsync(int idProducto, decimal nuevoPrecio, decimal nuevoCosto)
    {
        if (nuevoPrecio < nuevoCosto)
            throw new PrecioInvalidoException();

        var producto = await _repo.ObtenerPorIdAsync(idProducto)
            ?? throw new EntidadNoEncontradaException("Producto", idProducto);

        producto.Precio_unitario = nuevoPrecio;
        producto.Costo_unitario  = nuevoCosto;
        await _repo.ActualizarAsync(producto);
    }

    public async Task<IEnumerable<Producto>> ObtenerTodosAsync(int? idCategoria = null, bool soloConStock = false)
        => await _repo.ObtenerTodosAsync(idCategoria, soloConStock);

    public async Task<int> AjustarDeInventarioAsync(AjustarStockRequest req)
    {
        // Usamos _repo que es el nombre que definiste arriba
        return await _repo.AjustarInventarioAsync(req);
    }

    public async Task<IEnumerable<ProductoFiltroDTO>> FiltrarPorCategoriaAsync(int id)
    {
        return await _repo.ObtenerPorCategoriaAsync(id);
    }

    public async Task<bool> ActualizarPreciosEnLoteAsync(ActualizarPreciosLoteRequest request)
    {
        if (request.Precios == null || !request.Precios.Any()) return false;

        foreach (var item in request.Precios)
        {
            var producto = await _repo.ObtenerPorIdAsync(item.IdProducto);
            if (producto != null)
            {
                producto.Precio_unitario= item.NuevoPrecio;
                await _repo.ActualizarAsync(producto);
            }
        }
        return true;
    }
    public async Task<IEnumerable<ProductoRankingDTO>> ObtenerRankingProductosAsync()
    {
        
        return await _repo.ObtenerRankingUsoAsync(10);
    }

    public async Task<IEnumerable<ProductoBajoStockDTO>> ObtenerAlertasStockAsync()
    {
        var productosBajoStock = await _repo.ObtenerBajoStockDesdeSPAsync(5);

        return productosBajoStock.Select(p => new ProductoBajoStockDTO(
            p.Id_producto,
            p.Descripcion_producto,
            p.Stock_actual,
            5, 
            "Alerta: Stock crítico detectado por el sistema."
        )).ToList();
    }
}
