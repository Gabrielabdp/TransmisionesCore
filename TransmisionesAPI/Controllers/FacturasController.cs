using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaUseCases _useCases;
    private readonly TransmisionesContext _context;

    public FacturasController(FacturaUseCases useCases, TransmisionesContext context)
    {
        _useCases = useCases;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Generar(int idOrden, int idEmpleado, int idCaja, string tipoFactura = "Consumidor_Final")
        => Ok(await _useCases.GenerarFacturaAsync(idOrden, idEmpleado, idCaja, tipoFactura));

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerFactura(int id)
    {
        var factura = await _context.Facturas
            .Include(f => f.Orden)
                .ThenInclude(o => o.DetallesOrden)
                    .ThenInclude(d => d.Producto)
            .Include(f => f.Orden)
                .ThenInclude(o => o.DetallesServicio)
                    .ThenInclude(ds => ds.Servicio)
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id_factura == id);

        if (factura == null)
            return NotFound(new { mensaje = $"Factura {id} no encontrada." });

        var resultado = new
        {
            factura.Id_factura,
            factura.Numero_factura,
            factura.Fecha_factura,
            factura.SubTotal,
            factura.ITBIS,
            factura.Total,
            factura.Tipo_factura,
            factura.Estado,
            factura.Id_cliente,
            NombreCliente = factura.Cliente != null
                ? $"{factura.Cliente.Nombre_cliente} {factura.Cliente.Apellido_cliente}"
                : "—",
            CorreoCliente = factura.Cliente?.Correo_cliente,
            Detalles = factura.Orden?.DetallesOrden?.Select(d => new
            {
                Descripcion = d.Producto?.Descripcion_producto ?? "Producto",
                d.Cantidad,
                PrecioUnitario = d.Precio_unitario,
                Subtotal = d.SubTotal
            }).Cast<object>()
            .Concat(factura.Orden?.DetallesServicio?.Select(ds => new
            {
                Descripcion = ds.Servicio?.Nombre_servicio ?? "Servicio",
                Cantidad = 1,
                PrecioUnitario = ds.Precio_cobrado,
                Subtotal = ds.SubTotal
            }).Cast<object>() ?? Enumerable.Empty<object>())
            .ToList()
        };

        return Ok(resultado);
    }
}
