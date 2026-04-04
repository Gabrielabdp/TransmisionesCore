using Microsoft.EntityFrameworkCore;
using TransmisionesCore.Interfaces;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;

namespace TransmisionesAPI;

public static class PruebasEndpoints
{
    public static void MapPruebasDb(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/debug");

        // 1. Prueba de Integridad Geográfica (Lo que ya hicimos)
        group.MapGet("/cliente-full", async (TransmisionesContext db) =>
        {
            return await db.Clientes
                .Include(c => c.Sector)
                    .ThenInclude(s => s.Municipio)
                        .ThenInclude(m => m.Provincia)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id_cliente == 1);
        });

        // 2. Prueba de Flujo de Taller (Orden -> Vehículo)
        group.MapGet("/orden-check", async (TransmisionesContext db) =>
        {
            // Buscamos la última orden para ver si cargan sus relaciones
            var orden = await db.Ordenes
                .Include(o => o.Vehiculo)
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.Id_orden)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return orden is not null ? Results.Ok(orden) : Results.NotFound("No hay órdenes");
        });

        // 3. Prueba de Inventario y Decimales
        group.MapGet("/producto-precios", async (TransmisionesContext db) =>
        {
            var producto = await db.Productos
                .Select(p => new { p.Descripcion_producto, p.Precio_unitario })
                .FirstOrDefaultAsync();
            return Results.Ok(producto);
        });

        // 4. Buscar Cliente por Cédula o RNC
        group.MapGet("/buscar-cliente/{documento}", async (string documento, TransmisionesContext db) =>
        {
            var cliente = await db.Clientes
                .Where(c => c.Cedula_cliente == documento || c.RNC_cliente == documento)
                .Select(c => new { c.Nombre_cliente, c.Apellido_cliente, c.Telefono_cliente })
                .FirstOrDefaultAsync();

            return cliente is not null ? Results.Ok(cliente) : Results.NotFound("Cliente no encontrado con ese documento.");
        });

        // 5. Alerta de Stock (Productos con menos de 5 unidades)
        group.MapGet("/stock-critico", async (TransmisionesContext db) =>
        {
            var alertas = await db.Productos
                .Where(p => p.Stock_actual <= 5) // Asumiendo que tienes esta columna
                .Select(p => new { p.Descripcion_producto, p.Stock_actual })
                .ToListAsync();

            return Results.Ok(new { Mensaje = "Revisar estos repuestos", Data = alertas });
        });

        // 6. Estado Real de la Caja Activa
        group.MapGet("/caja-status", async (TransmisionesContext db) =>
        {
            var caja = await db.Cajas
                .Select(c => new { c.Codigo_caja, c.Estado, c.Saldo_final, c.Ultima_apertura })
                .FirstOrDefaultAsync(c => c.Codigo_caja == "CAJA-001");

            return caja is not null ? Results.Ok(caja) : Results.NotFound("Caja no configurada.");
        });

        // 7. Historial de órdenes por placa
        group.MapGet("/historial/{placa}", async (string placa, TransmisionesContext db) =>
        {
            var historial = await db.Ordenes
                .Where(o => o.Id_vehiculo == placa)
                .Select(o => new { o.Id_orden, o.Fecha_orden, o.Total_orden })
                .ToListAsync();

            return Results.Ok(new { Vehiculo = placa, TotalOrdenes = historial.Count, Detalle = historial });
        });

        // 8. Prueba Manual de Logs
        group.MapPost("/test-log", async (ILogService log) =>
        {
            // Intentamos registrar un evento ficticio
            await log.RegistrarLogAsync(
                accion: "PRUEBA_SISTEMA",
                tabla: "Debug",
                detalle: "Probando que los logs de Gabriela funcionan desde la API",
                idUsuario: 1 // Pon un ID de usuario que exista en tu tabla Usuario
            );

            return Results.Ok(new { Mensaje = "Log enviado a la base de datos con éxito" });
        });

        // 9. Prueba de Ajuste de Stock 
        group.MapPost("/test-ajuste-stock", async (AjustarStockRequest req, ProductoUseCases useCases) =>
        {
            try
            {
               
                var nuevoStock = await useCases.AjustarDeInventarioAsync(req);

                return Results.Ok(new
                {
                    Mensaje = "Prueba de SP en Azure exitosa",
                    StockFinal = nuevoStock
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        });
    }
}
