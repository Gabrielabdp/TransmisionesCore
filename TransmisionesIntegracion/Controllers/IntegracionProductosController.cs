using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/productos")]
    [ApiController]
    public class IntegracionProductosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionProductosController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCatalogo([FromQuery] int? idCategoria, [FromQuery] bool soloConStock = false)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var urlCore = $"https://localhost:56678/api/Productos?soloConStock={soloConStock}";
                if (idCategoria.HasValue) urlCore += $"&idCategoria={idCategoria.Value}";

                var respuestaCore = await cliente.GetAsync(urlCore);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    var jsonCore = await respuestaCore.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productosDelCore = JsonSerializer.Deserialize<List<ProductoCoreDto>>(jsonCore, opciones);

                    if (productosDelCore != null && !idCategoria.HasValue && !soloConStock)
                    {
                        // Solo refrescamos el caché global si trajimos la lista completa
                        _context.ProductosCache.RemoveRange(_context.ProductosCache);
                        foreach (var p in productosDelCore)
                        {
                            _context.ProductosCache.Add(new ProductoCache
                            {
                                Id = p.Id_producto,
                                Descripcion = p.Descripcion_producto,
                                PrecioUnitario = p.Precio_unitario,
                                StockActual = p.Stock_actual,
                                UltimaActualizacion = DateTime.Now
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    return Content(jsonCore, "application/json");
                }
                else
                {
                    return await ResponderDesdeCache(soloConStock);
                }
            }
            catch (Exception)
            {
                return await ResponderDesdeCache(soloConStock);
            }
        }

        private async Task<IActionResult> ResponderDesdeCache(bool soloConStock)
        {
            var consulta = _context.ProductosCache.AsQueryable();

            if (soloConStock)
            {
                consulta = consulta.Where(p => p.StockActual > 0);
            }

            var productosCacheados = await consulta.ToListAsync();

            if (productosCacheados.Any())
            {
                return Ok(new { modoOffline = true, mensaje = "Sistema desconectado. Mostrando inventario local.", datos = productosCacheados });
            }
            return StatusCode(503, "El sistema central está caído y no hay datos locales.");
        }


        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] CrearProductoIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var urlCore = "https://localhost:56678/api/Productos";

                var jsonContent = new StringContent(JsonSerializer.Serialize(peticion), Encoding.UTF8, "application/json");
                var respuestaCore = await cliente.PostAsync(urlCore, jsonContent);

                if (respuestaCore.IsSuccessStatusCode)
                    return Ok(new { exito = true, mensaje = "Producto registrado en el sistema central." });
                else
                    return await GuardarEnColaOffline("NuevoProducto", peticion);
            }
            catch (Exception)
            {
                return await GuardarEnColaOffline("NuevoProducto", peticion);
            }
        }


        [HttpPatch("{id}/precio")]
        public async Task<IActionResult> ActualizarPrecio(int id, [FromQuery] decimal nuevoPrecio, [FromQuery] decimal? nuevoCosto)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var urlCore = $"https://localhost:56678/api/Productos/{id}/precio?nuevoPrecio={nuevoPrecio}";
                if (nuevoCosto.HasValue) urlCore += $"&nuevoCosto={nuevoCosto.Value}";

                var respuestaCore = await cliente.PatchAsync(urlCore, null);

                if (respuestaCore.IsSuccessStatusCode)
                    return Ok(new { exito = true, mensaje = "Precio actualizado en el sistema central." });
                else
                    return await GuardarPatchOffline(id, nuevoPrecio, nuevoCosto);
            }
            catch (Exception)
            {
                return await GuardarPatchOffline(id, nuevoPrecio, nuevoCosto);
            }
        }

        private async Task<IActionResult> GuardarEnColaOffline(string tipo, CrearProductoIntegracionDto datos)
        {
            // 1. Al buzón
            var jsonEmpacado = JsonSerializer.Serialize(datos);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = tipo,
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now
            });

            // 2. A la vitrina temporal (Caché Optimista)
            var idFalso = _context.ProductosCache.Any() ? _context.ProductosCache.Min(p => p.Id) - 1 : -1;
            if (idFalso >= 0) idFalso = -1;

            _context.ProductosCache.Add(new ProductoCache
            {
                Id = idFalso,
                Descripcion = datos.Descripcion + " (Pendiente de Sincronizar)",
                PrecioUnitario = datos.PrecioUnitario,
                StockActual = datos.StockInicial,
                UltimaActualizacion = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = "Producto guardado localmente. Se sincronizará luego." });
        }

        [HttpPost("ajustar-stock")]
        public async Task<IActionResult> AjustarStock([FromBody] AjustarStockIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(5);
                var urlCore = "https://localhost:56678/api/Productos/ajustar-stock";

                var jsonContent = new StringContent(JsonSerializer.Serialize(peticion), Encoding.UTF8, "application/json");
                var respuestaCore = await cliente.PostAsync(urlCore, jsonContent);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    var jsonRespuesta = await respuestaCore.Content.ReadAsStringAsync();
                    return Content(jsonRespuesta, "application/json");
                }

                // Si el CORE rechaza por regla de negocio (ej. "Stock insuficiente")
                if (respuestaCore.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return BadRequest(new
                    {
                        exito = false,
                        mensaje = "El CORE rechazó el ajuste de inventario.",
                        detalle = await respuestaCore.Content.ReadAsStringAsync()
                    });
                }

                return await GuardarAjusteStockOffline(peticion);
            }
            catch (Exception)
            {
                return await GuardarAjusteStockOffline(peticion);
            }
        }

        private async Task<IActionResult> GuardarAjusteStockOffline(AjustarStockIntegracionDto datos)
        {
            var productoLocal = await _context.ProductosCache.FirstOrDefaultAsync(p => p.Id == datos.IdProducto);

            if (productoLocal == null)
                return NotFound(new { mensaje = "(Offline) El producto no existe en la caché local." });

            // SANITIZACIÓN: Convertimos la cantidad a positivo absoluto para evitar que "Menos por Menos sea Más"
            int cantidadOperacion = Math.Abs(datos.Cantidad);

            // 1. Caché Optimista con Reglas de Negocio
            if (datos.TipoAjuste == "Entrada")
            {
                productoLocal.StockActual += cantidadOperacion;
            }
            else if (datos.TipoAjuste == "Salida" || datos.TipoAjuste == "Danado")
            {
                if (productoLocal.StockActual < cantidadOperacion)
                {
                    return BadRequest(new { mensaje = "(Offline) Stock local insuficiente para realizar esta salida." });
                }
                productoLocal.StockActual -= cantidadOperacion;
            }
            else // Ajuste_Manual (Reemplazo absoluto por el número exacto que enviaron)
            {
                // El ajuste manual reemplaza el stock con el número enviado (no aplica Math.Abs aquí por si envían 0)
                productoLocal.StockActual = datos.Cantidad < 0 ? 0 : datos.Cantidad;
            }

            productoLocal.UltimaActualizacion = DateTime.Now;

            // 2. Al buzón de salida enviamos el JSON original (o podemos forzar que el JSON lleve el número sanitizado)
            // Es mejor limpiar el objeto antes de guardarlo en SQLite para que el CORE no reciba el error tampoco
            datos.Cantidad = datos.TipoAjuste == "Ajuste_Manual" ? datos.Cantidad : cantidadOperacion;

            var jsonEmpacado = JsonSerializer.Serialize(datos);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = "AjustarStockProducto",
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now,
                Sincronizado = false
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                modoOffline = true,
                mensaje = "Inventario ajustado en caché local. Se sincronizará luego.",
                stockActual = productoLocal.StockActual
            });
        }
        private async Task<IActionResult> GuardarPatchOffline(int id, decimal precio, decimal? costo)
        {
            var datosPatch = new { IdProducto = id, NuevoPrecio = precio, NuevoCosto = costo };

            // 1. Al buzón
            var jsonEmpacado = JsonSerializer.Serialize(datosPatch);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = "ActualizarPrecioProducto",
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now
            });

            // 2. Actualizamos la vitrina inmediatamente para que la caja vea el nuevo precio
            var productoLocal = _context.ProductosCache.FirstOrDefault(p => p.Id == id);
            if (productoLocal != null)
            {
                productoLocal.PrecioUnitario = precio;
                productoLocal.UltimaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = "Precio actualizado en caché local." });
        }
    }


    public class CrearProductoIntegracionDto
    {
        public int IdCategoria { get; set; }
        public int IdTipoTrans { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Marca { get; set; } = string.Empty;
        public int StockInicial { get; set; }
    }

    public class AjustarStockIntegracionDto
    {
        public int IdProducto { get; set; }
        public int IdEmpleado { get; set; }
        public string TipoAjuste { get; set; } = string.Empty; // Entrada, Salida, Danado, Ajuste_Manual
        public int Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class ProductoCoreDto
    {
        public int Id_producto { get; set; }
        public string Descripcion_producto { get; set; } = string.Empty;
        public decimal Precio_unitario { get; set; }
        public int Stock_actual { get; set; }
    }
}