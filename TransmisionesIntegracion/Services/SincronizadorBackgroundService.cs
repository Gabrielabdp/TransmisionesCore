using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Controllers;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Services
{
    public class SincronizadorBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;

        public SincronizadorBackgroundService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SincronizarColaAsync();
                await RefrescarCachesGlobalesAsync();
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task RefrescarCachesGlobalesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IntegracionDbContext>();
            var cliente = _httpClientFactory.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(15);

            try
            {
                // === 1. CLONAR PRODUCTOS ===
                var resProductos = await cliente.GetAsync("https://localhost:56678/api/Productos");
                if (resProductos.IsSuccessStatusCode)
                {
                    var json = await resProductos.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productos = JsonSerializer.Deserialize<List<ProductoCoreDtoSync>>(json, opciones);
                    if (productos != null)
                    {
                        context.ProductosCache.RemoveRange(context.ProductosCache);
                        foreach (var p in productos)
                        {
                            context.ProductosCache.Add(new ProductoCache
                            {
                                Id = p.Id_producto,
                                Descripcion = p.Descripcion_producto,
                                PrecioUnitario = p.Precio_unitario,
                                StockActual = p.Stock_actual,
                                UltimaActualizacion = DateTime.Now
                            });
                        }
                    }
                }

                // === 2. CLONAR CLIENTES ===
                var resClientes = await cliente.GetAsync("https://localhost:56678/api/Clientes");
                if (resClientes.IsSuccessStatusCode)
                {
                    var json = await resClientes.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var clientes = JsonSerializer.Deserialize<List<ClienteCoreDtoSync>>(json, opciones);
                    if (clientes != null)
                    {
                        context.ClientesCache.RemoveRange(context.ClientesCache);
                        foreach (var c in clientes)
                        {
                            context.ClientesCache.Add(new ClienteCache
                            {
                                Id = c.Id_cliente,
                                Nombre = c.Nombre_cliente,
                                Apellido = c.Apellido_cliente,
                                Documento = !string.IsNullOrEmpty(c.RNC_cliente) ? c.RNC_cliente : c.Cedula_cliente ?? "",
                                Telefono = c.Telefono_cliente ?? ""
                            });
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
            catch (Exception) { /* Ignorar errores de red en el refresco de caché */ }
        }

        private async Task SincronizarColaAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IntegracionDbContext>();
            var pendientes = context.TransaccionesPendientes.Where(t => !t.Sincronizado).ToList();

            if (!pendientes.Any()) return;

            var cliente = _httpClientFactory.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(15);

            foreach (var transaccion in pendientes)
            {
                try
                {
                    bool exitoAlEnviar = false;
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    if (transaccion.TipoTransaccion == "AperturaCaja")
                    {
                        var paquete = JsonSerializer.Deserialize<PaqueteAperturaOffline>(transaccion.DatosJson, opciones);
                        if (paquete != null && paquete.DatosApertura != null)
                        {
                            var urlCore = $"https://localhost:56678/api/Cajas/{paquete.IdCaja}/abrir?idUsuario={paquete.DatosApertura.IdUsuario}&saldoInicial={paquete.DatosApertura.SaldoInicial}";
                            var respuesta = await cliente.PostAsync(urlCore, null);
                            if (respuesta.IsSuccessStatusCode) exitoAlEnviar = true;
                        }
                    }
                    else if (transaccion.TipoTransaccion == "CierreCaja")
                    {
                        var paquete = JsonSerializer.Deserialize<PaqueteCierreOffline>(transaccion.DatosJson, opciones);
                        if (paquete != null && paquete.DatosApertura != null)
                        {
                            var urlCore = $"https://localhost:56678/api/Cajas/{paquete.IdCaja}/cerrar?idUsuario={paquete.DatosApertura.IdUsuario}&saldoFinal={paquete.DatosApertura.SaldoFinal}";
                            var respuesta = await cliente.PostAsync(urlCore, null);
                            if (respuesta.IsSuccessStatusCode) exitoAlEnviar = true;
                        }
                    }
                    else if (transaccion.TipoTransaccion == "MovimientoCaja")
                    {
                        var paquete = JsonSerializer.Deserialize<PaqueteMovimientoOffline>(transaccion.DatosJson, opciones);
                        if (paquete != null && paquete.DatosMovimiento != null)
                        {
                            var urlCore = $"https://localhost:56678/api/Cajas/{paquete.IdCaja}/movimiento?idUsuario={paquete.DatosMovimiento.IdUsuario}&monto={paquete.DatosMovimiento.Monto}&tipo={paquete.DatosMovimiento.Tipo}&motivo={paquete.DatosMovimiento.Motivo}";
                            var respuesta = await cliente.PostAsync(urlCore, null);
                            if (respuesta.IsSuccessStatusCode) exitoAlEnviar = true;
                        }
                    }

                    if (exitoAlEnviar)
                    {
                        context.TransaccionesPendientes.Remove(transaccion);
                    }
                }
                catch (Exception) { break; }
            }

            await context.SaveChangesAsync();
        }

        // Clases DTO para Deserialización Offline
        private class PaqueteAperturaOffline
        {
            public int IdCaja { get; set; }
            public DatosApertura? DatosApertura { get; set; }
        }

        private class PaqueteCierreOffline
        {
            public int IdCaja { get; set; }
            public DatosCierre? DatosApertura { get; set; } 
        }

        private class PaqueteMovimientoOffline
        {
            public int IdCaja { get; set; }
            public DatosMovimiento? DatosMovimiento { get; set; }
        }

        private class DatosApertura
        {
            public int IdUsuario { get; set; }
            public decimal SaldoInicial { get; set; }
        }

        private class DatosCierre
        {
            public int IdUsuario { get; set; }
            public decimal SaldoFinal { get; set; }
        }

        private class DatosMovimiento
        {
            public int IdUsuario { get; set; }
            public decimal Monto { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string Motivo { get; set; } = string.Empty;
        }

        private class ProductoCoreDtoSync
        {
            public int Id_producto { get; set; }
            public string Descripcion_producto { get; set; } = string.Empty;
            public decimal Precio_unitario { get; set; }
            public int Stock_actual { get; set; }
        }

        private class ClienteCoreDtoSync
        {
            public int Id_cliente { get; set; }
            public string Nombre_cliente { get; set; } = string.Empty;
            public string Apellido_cliente { get; set; } = string.Empty;
            public string? RNC_cliente { get; set; }
            public string? Cedula_cliente { get; set; }
            public string? Telefono_cliente { get; set; }
        }
    }
}
