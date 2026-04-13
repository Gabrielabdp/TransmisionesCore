using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/ordenes")]
    [ApiController]
    public class IntegracionOrdenesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionOrdenesController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerOrdenes([FromQuery] string? estado, [FromQuery] int? idCliente)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var urlCore = $"https://localhost:56678/api/Ordenes?";
                if (!string.IsNullOrEmpty(estado)) urlCore += $"estado={estado}&";
                if (idCliente.HasValue) urlCore += $"idCliente={idCliente.Value}";

                var respuesta = await cliente.GetAsync(urlCore);
                if (respuesta.IsSuccessStatusCode)
                {
                    return Content(await respuesta.Content.ReadAsStringAsync(), "application/json");
                }
                return ResponderOrdenesDesdeCache(estado, idCliente, false);
            }
            catch (Exception)
            {
                return ResponderOrdenesDesdeCache(estado, idCliente, false);
            }
        }

        [HttpGet("cotizaciones")]
        public async Task<IActionResult> ObtenerCotizaciones([FromQuery] int? idCliente)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var urlCore = $"https://localhost:56678/api/Ordenes/cotizaciones?";
                if (idCliente.HasValue) urlCore += $"idCliente={idCliente.Value}";

                var respuesta = await cliente.GetAsync(urlCore);
                if (respuesta.IsSuccessStatusCode)
                    return Content(await respuesta.Content.ReadAsStringAsync(), "application/json");

                return ResponderOrdenesDesdeCache(null, idCliente, true);
            }
            catch (Exception)
            {
                return ResponderOrdenesDesdeCache(null, idCliente, true);
            }
        }

        // Auxiliar para no repetir código
        private IActionResult ResponderOrdenesDesdeCache(string? estado, int? idCliente, bool soloCotizaciones)
        {
            var consulta = _context.OrdenesCache.AsQueryable();

            if (soloCotizaciones) consulta = consulta.Where(o => o.TipoOrden == "Cotizacion");
            else if (!string.IsNullOrEmpty(estado)) consulta = consulta.Where(o => o.Estado.Contains(estado));

            if (idCliente.HasValue) consulta = consulta.Where(o => o.IdCliente == idCliente.Value);

            var resultados = consulta.OrderByDescending(o => o.Fecha).ToList();
            return Ok(new { modoOffline = true, datos = resultados });
        }

        [HttpPost("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarOrden(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var respuesta = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{id}/confirmar", null);

                if (respuesta.IsSuccessStatusCode) return Ok(new { exito = true, mensaje = "Orden confirmada en el sistema central." });

                return await GuardarAccionOrdenOffline("ConfirmarOrden", id);
            }
            catch (Exception)
            {
                return await GuardarAccionOrdenOffline("ConfirmarOrden", id);
            }
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarOrden(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var respuesta = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{id}/cancelar", null);

                if (respuesta.IsSuccessStatusCode) return Ok(new { exito = true, mensaje = "Orden cancelada en el sistema central." });

                return await GuardarAccionOrdenOffline("CancelarOrden", id);
            }
            catch (Exception)
            {
                return await GuardarAccionOrdenOffline("CancelarOrden", id);
            }
        }

        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> AprobarCotizacion(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{id}/aprobar", new StringContent(""));

                if (res.IsSuccessStatusCode) return Content(await res.Content.ReadAsStringAsync(), "application/json");
                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest) return BadRequest(await res.Content.ReadAsStringAsync());
            }
            catch (Exception) { /* Ignoramos para caer a modo offline */ }

            // FILTRO OFFLINE: Si no hay red, verificamos que tenga sentido encolarlo
            var ordenLocal = _context.OrdenesCache.FirstOrDefault(o => o.Id == id);
            if (ordenLocal != null && (ordenLocal.TipoOrden != "Cotizacion" || ordenLocal.TipoOrden != "Cotización"))
            {
                return BadRequest(new { mensaje = "(Offline) Rechazado: Solo se pueden aprobar Cotizaciones." });
            }

            return await EncolarTransaccionOffline("AprobarCotizacion", new { IdOrden = id });
        }

        [HttpPost("{id}/convertir")]
        public async Task<IActionResult> ConvertirCotizacion(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{id}/convertir", new StringContent(""));

                if (res.IsSuccessStatusCode) return Content(await res.Content.ReadAsStringAsync(), "application/json");
                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest) return BadRequest(await res.Content.ReadAsStringAsync());
            }
            catch (Exception) { /* Caer a modo offline */ }

            // FILTRO OFFLINE
            var ordenLocal = _context.OrdenesCache.FirstOrDefault(o => o.Id == id);
            if (ordenLocal != null && ordenLocal.TipoOrden == "Factura")
            {
                return BadRequest(new { mensaje = "(Offline) Rechazado: Esta orden ya es una Factura." });
            }

            return await EncolarTransaccionOffline("ConvertirAFactura", new { IdOrden = id });
        }

        // CORRECCIÓN: Quitamos el [FromBody] string motivoAnulacion
        [HttpPost("{id}/anular")]
        public async Task<IActionResult> AnularOrden(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{id}/anular", new StringContent(""));

                if (res.IsSuccessStatusCode) return Content(await res.Content.ReadAsStringAsync(), "application/json");
                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest) return BadRequest(await res.Content.ReadAsStringAsync());
            }
            catch (Exception) { /* Caer a modo offline */ }

            // FILTRO OFFLINE
            var ordenLocal = _context.OrdenesCache.FirstOrDefault(o => o.Id == id);
            if (ordenLocal != null && (ordenLocal.Estado == "Cancelada" || ordenLocal.Estado == "Anulada"))
            {
                return BadRequest(new { mensaje = "(Offline) Rechazado: Esta orden ya se encuentra anulada." });
            }

            return await EncolarTransaccionOffline("AnularOrden", new { IdOrden = id });
        }

        [HttpPatch("{id}/asignar-empleado")]
        public async Task<IActionResult> AsignarEmpleado(int id, [FromBody] int idEmpleado)
        {
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(new { idEmpleado = idEmpleado });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var res = await cliente.PatchAsync($"https://localhost:56678/api/Ordenes/{id}/asignar-empleado", content);

                if (res.IsSuccessStatusCode) return Content(await res.Content.ReadAsStringAsync(), "application/json");
                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest) return BadRequest(await res.Content.ReadAsStringAsync());
            }
            catch (Exception) { /* Caer a modo offline */ }

            // FILTRO OFFLINE AVANZADO
            var ordenLocal = _context.OrdenesCache.FirstOrDefault(o => o.Id == id);
            if (ordenLocal != null)
            {
                if (ordenLocal.Estado == "Cancelada" || ordenLocal.Estado == "Anulada")
                {
                    return BadRequest(new { mensaje = "(Offline) Rechazado: No se puede asignar un mecánico a una orden que ha sido anulada." });
                }
                if (ordenLocal.Estado == "Facturada" || ordenLocal.Estado == "Completada")
                {
                    return BadRequest(new { mensaje = "(Offline) Rechazado: No se puede reasignar un empleado a una orden ya finalizada." });
                }
            }

            return await EncolarTransaccionOffline("AsignarEmpleadoOrden", new { IdOrden = id, IdEmpleado = idEmpleado });
        }

        private async Task<IActionResult> GuardarAccionOrdenOffline(string tipoAccion, int idOrden)
        {
            var jsonEmpacado = JsonSerializer.Serialize(new { IdOrden = idOrden });
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = tipoAccion,
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = $"La acción '{tipoAccion}' se ha encolado localmente." });
        }


        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarOrdenCompleta([FromBody] OrdenCompletaDto carrito)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(20); // Damos 20 seg porque hará varias operaciones

                // Intentamos procesarla online directamente
                bool exito = await ProcesarEnCore(cliente, carrito);

                if (exito)
                {
                    return Ok(new { exito = true, mensaje = "Orden y Factura procesadas en el sistema central." });
                }
                else
                {
                    return await GuardarOrdenOffline(carrito);
                }
            }
            catch (Exception)
            {
                // Modo Offline
                return await GuardarOrdenOffline(carrito);
            }
        }

        private async Task<bool> ProcesarEnCore(HttpClient cliente, OrdenCompletaDto carrito)
        {
            // 1. CREAR LA ORDEN
            var urlCrearOrden = "https://localhost:56678/api/Ordenes";
            var datosOrden = new
            {
                idCliente = carrito.IdCliente,
                idEmpleado = carrito.IdEmpleado,
                idCanal = carrito.IdCanal,
                tipoOrden = carrito.TipoOrden,
                estadoOrden = "Pendiente",
                idCondicionPago = carrito.IdCondicionPago,
                idVehiculo = carrito.IdVehiculo,
                fechaVencimientoCotizacion = carrito.FechaVencimientoCotizacion?.ToString("O") ?? DateTime.Now.AddDays(15).ToString("O")
            };

            var jsonOrden = new StringContent(JsonSerializer.Serialize(datosOrden), Encoding.UTF8, "application/json");
            var resOrden = await cliente.PostAsync(urlCrearOrden, jsonOrden);

            if (!resOrden.IsSuccessStatusCode) return false;

            // Tratamos de leer el ID de la orden que nos devolvió el CORE
            var idOrdenStr = await resOrden.Content.ReadAsStringAsync();
            // Busca cualquier secuencia de dígitos (\d+) ignorando comillas o letras
            var match = System.Text.RegularExpressions.Regex.Match(idOrdenStr, @"\d+");
            if (!match.Success) return false;

            int idOrdenNueva = int.Parse(match.Value);

            // Variable para detectar si algo salió mal en los detalles
            bool errorEnDetalles = false;

            // 2. AGREGAR PRODUCTOS
            foreach (var prod in carrito.Productos)
            {
                var urlProducto = "https://localhost:56678/api/Ordenes/producto";
                var datosProd = new { idOrden = idOrdenNueva, idProducto = prod.IdProducto, cantidad = prod.Cantidad };
                var jsonProd = new StringContent(JsonSerializer.Serialize(datosProd), Encoding.UTF8, "application/json");
                var resProd = await cliente.PostAsync(urlProducto, jsonProd);

                if (!resProd.IsSuccessStatusCode) errorEnDetalles = true;
            }

            // 3. AGREGAR SERVICIOS (Si los hay)
            foreach (var serv in carrito.Servicios)
            {
                var urlServicio = "https://localhost:56678/api/Ordenes/servicio";
                var datosServ = new { idOrden = idOrdenNueva, idServicio = serv.IdServicio, idEmpleadoTecnico = serv.IdEmpleadoTecnico, precioCobrado = serv.PrecioCobrado, descripcionTrabajo = serv.DescripcionTrabajo };
                var jsonServ = new StringContent(JsonSerializer.Serialize(datosServ), Encoding.UTF8, "application/json");
                var resServ = await cliente.PostAsync(urlServicio, jsonServ);

                if (!resServ.IsSuccessStatusCode) errorEnDetalles = true;
            }

            // ROLLBACK COMPENSATORIO
            if (errorEnDetalles)
            {
                // Algo falló (ej. no hay stock). Cancelamos la orden que acabamos de crear.
                await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrdenNueva}/cancelar", null);
                return false; // Devolvemos false para que Integración sepa que falló
            }

            // 4 y 5. CONFIRMACIÓN Y FACTURACIÓN INTELIGENTE
            if (carrito.TipoOrden == "Factura")
            {
                await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrdenNueva}/confirmar", null);
                var urlFactura = $"https://localhost:56678/api/Facturas?idOrden={idOrdenNueva}&idEmpleado={carrito.IdEmpleado}&idCaja={carrito.IdCaja}&tipoFactura=Consumidor_Final";
                await cliente.PostAsync(urlFactura, null);
            }

            return true;
        }

        private async Task<IActionResult> GuardarOrdenOffline(OrdenCompletaDto carrito)
        {
            // 1. Al buzón
            var jsonEmpacado = JsonSerializer.Serialize(carrito);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = "OrdenCompleta",
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now
            });

            // 2. A la Vitrina (Caché Optimista)
            var idFalso = _context.OrdenesCache.Any() ? _context.OrdenesCache.Min(o => o.Id) - 1 : -1;
            if (idFalso >= 0) idFalso = -1;

            _context.OrdenesCache.Add(new OrdenCache
            {
                Id = idFalso,
                IdCliente = carrito.IdCliente,
                TipoOrden = carrito.TipoOrden,
                Estado = "Pendiente (Offline)",
                Fecha = DateTime.Now,
                TotalEstimado = 0 // En una app real sumarías los precios de carrito.Productos aquí
            });

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = "Sistema desconectado. Guardada localmente." });
        }

        private async Task<IActionResult> EncolarTransaccionOffline(string tipoAccion, object payload)
        {
            // Empacamos cualquier objeto anónimo que nos llegue (ej. { IdOrden = 1, Motivo = "Cliente canceló" })
            var jsonEmpacado = JsonSerializer.Serialize(payload);

            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = tipoAccion,
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now,
                Sincronizado = false // Igual que en tu controlador de Clientes
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                modoOffline = true,
                exito = true,
                mensaje = $"Sistema desconectado. La acción '{tipoAccion}' se ha guardado en la cola para sincronización."
            });
        }

    }


    public class OrdenCompletaDto
    {
        public int IdCliente { get; set; }
        public int IdEmpleado { get; set; }
        public int IdCanal { get; set; }
        public string TipoOrden { get; set; } = "Factura";
        public int IdCondicionPago { get; set; }
        public string IdVehiculo { get; set; } = "N/A";
        public int IdCaja { get; set; } // Necesario para la facturación final
        public DateTime? FechaVencimientoCotizacion { get; set; }

        public List<DetalleProductoDto> Productos { get; set; } = new();
        public List<DetalleServicioDto> Servicios { get; set; } = new();
    }

    public class DetalleProductoDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }

    public class DetalleServicioDto
    {
        public int IdServicio { get; set; }
        public int IdEmpleadoTecnico { get; set; }
        public decimal PrecioCobrado { get; set; }
        public string DescripcionTrabajo { get; set; } = string.Empty;
    }
}