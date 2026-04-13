using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Controllers;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Services
{
    // Heredar de BackgroundService lo convierte en un proceso que corre en bucle infinito
    public class SincronizadorBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SincronizadorBackgroundService> _logger;

        public SincronizadorBackgroundService(
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<SincronizadorBackgroundService> logger) 
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Enviamos lo que está pendiente en el buzón
                await SincronizarColaAsync();

                // 2. NUEVO: Traemos la verdad absoluta de Azure a nuestro caché
                await RefrescarCachesGlobalesAsync();

                // Esperamos 30 segundos
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
                        // Vaciamos la vitrina local por completo (borrando los IDs -1)
                        context.ProductosCache.RemoveRange(context.ProductosCache);

                        // Llenamos con los datos oficiales
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
                        // Vaciamos la vitrina local por completo (borrando los IDs -1)
                        context.ClientesCache.RemoveRange(context.ClientesCache);

                        // Llenamos con los datos oficiales
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
                // === 3. CLONAR ÓRDENES (CABECERAS) ===
                var resOrdenes = await cliente.GetAsync("https://localhost:56678/api/Ordenes");
                if (resOrdenes.IsSuccessStatusCode)
                {
                    var json = await resOrdenes.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    // Creamos un DTO temporal rápido usando JsonDocument para no hacer clases inmensas
                    using JsonDocument doc = JsonDocument.Parse(json);

                    context.OrdenesCache.RemoveRange(context.OrdenesCache); // Limpiamos vitrina

                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        context.OrdenesCache.Add(new OrdenCache
                        {
                            Id = element.GetProperty("id_orden").GetInt32(),
                            IdCliente = element.GetProperty("id_cliente").GetInt32(),
                            TipoOrden = element.GetProperty("tipo_orden").GetString() ?? "",
                            Estado = element.GetProperty("estado_orden").GetString() ?? "",
                            Fecha = element.GetProperty("fecha_orden").GetDateTime(),
                            TotalEstimado = element.GetProperty("total_orden").GetDecimal()
                        });
                    }
                }

                // === 4. CLONAR SERVICIOS ===
                var resServicios = await cliente.GetAsync("https://localhost:56678/api/Servicios");
                if (resServicios.IsSuccessStatusCode)
                {
                    var json = await resServicios.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(json);
                    context.ServiciosCache.RemoveRange(context.ServiciosCache); // Limpiamos vitrina

                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        context.ServiciosCache.Add(new ServicioCache
                        {
                            Id = element.GetProperty("id_servicio").GetInt32(),
                            Nombre = element.GetProperty("nombre_servicio").GetString() ?? "",
                            PrecioBase = element.GetProperty("precio_base").GetDecimal()
                        });
                    }
                }

                // === 5. CLONAR CATÁLOGOS (Condiciones de Pago) ===
                var resCondPago = await cliente.GetAsync("https://localhost:56678/api/Catalogos/condiciones-pago");
                if (resCondPago.IsSuccessStatusCode)
                {
                    var json = await resCondPago.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(json);
                    context.CondicionesPagoCache.RemoveRange(context.CondicionesPagoCache);

                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        context.CondicionesPagoCache.Add(new CondicionPagoCache
                        {
                            Id = element.GetProperty("id_condicion_pago").GetInt32(),
                            Descripcion = element.GetProperty("descripcion").GetString() ?? "",
                            Plazo = element.GetProperty("plazo").GetInt32()
                        });
                    }
                }

                // === 6. CLONAR CATÁLOGOS (Provincias y Municipios) ===
                var resProv = await cliente.GetAsync("https://localhost:56678/api/Catalogos/provincias");
                if (resProv.IsSuccessStatusCode)
                {
                    var jsonProv = await resProv.Content.ReadAsStringAsync();
                    using JsonDocument docProv = JsonDocument.Parse(jsonProv);

                    context.ProvinciasCache.RemoveRange(context.ProvinciasCache);
                    context.MunicipiosCache.RemoveRange(context.MunicipiosCache);

                    foreach (var element in docProv.RootElement.EnumerateArray())
                    {
                        int idProv = element.GetProperty("id_provincia").GetInt32();
                        context.ProvinciasCache.Add(new ProvinciaCache
                        {
                            Id = idProv,
                            Nombre = element.GetProperty("nombre_provincia").GetString() ?? ""
                        });

                        // Hacemos el fetch de los municipios de esa provincia
                        var resMuni = await cliente.GetAsync($"https://localhost:56678/api/Catalogos/municipios/{idProv}");
                        if (resMuni.IsSuccessStatusCode)
                        {
                            var jsonMuni = await resMuni.Content.ReadAsStringAsync();
                            using JsonDocument docMuni = JsonDocument.Parse(jsonMuni);
                            foreach (var m in docMuni.RootElement.EnumerateArray())
                            {
                                context.MunicipiosCache.Add(new MunicipioCache
                                {
                                    Id = m.GetProperty("id_municipio").GetInt32(),
                                    Nombre = m.GetProperty("nombre_municipio").GetString() ?? "",
                                    IdProvincia = idProv
                                });
                            }
                        }
                    }
                }

                // === 7. CLONAR EMPLEADOS Y USUARIOS ===
                var resEmpleados = await cliente.GetAsync("https://localhost:56678/api/Empleados");
                if (resEmpleados.IsSuccessStatusCode)
                {
                    var jsonEmp = await resEmpleados.Content.ReadAsStringAsync();
                    using JsonDocument docEmp = JsonDocument.Parse(jsonEmp);

                    context.EmpleadosCache.RemoveRange(context.EmpleadosCache); // Limpiamos vitrina

                    foreach (var element in docEmp.RootElement.EnumerateArray())
                    {
                        // Revisamos si este empleado tiene un usuario de sistema asociado
                        var propUsuario = element.GetProperty("usuario");
                        bool tieneUsuario = propUsuario.ValueKind != JsonValueKind.Null;

                        context.EmpleadosCache.Add(new EmpleadoCache
                        {
                            Id = element.GetProperty("id_empleado").GetInt32(),
                            NombreCompleto = $"{element.GetProperty("nombre").GetString()} {element.GetProperty("apellido").GetString()}",
                            Cedula = element.GetProperty("cedula").GetString() ?? "",
                            Activo = element.GetProperty("activo").GetBoolean(),

                            // Datos para el Login Offline
                            IdUsuario = tieneUsuario ? propUsuario.GetProperty("id_usuario").GetInt32() : null,
                            UsuarioAcceso = tieneUsuario ? propUsuario.GetProperty("nombre_usuario").GetString() ?? "" : "",
                            PasswordHash = tieneUsuario ? propUsuario.GetProperty("contrasena").GetString() ?? "" : "",
                            Rol = tieneUsuario ? propUsuario.GetProperty("rol").GetString() ?? "" : ""
                        });
                    }
                }

                // Guardamos todo el clonaje en SQLite
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Si hay un error de red, fallamos en silencio. 
                // La vitrina se queda con lo que tiene para seguir operando offline.
            }
        }

        private async Task SincronizarColaAsync()
        {

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IntegracionDbContext>();

            var pendientes = context.TransaccionesPendientes.Where(t => !t.Sincronizado).ToList();

            if (!pendientes.Any())
                return;

            var cliente = _httpClientFactory.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(15);

            foreach (var transaccion in pendientes)
            {
                try
                {
                    bool exitoAlEnviar = false;

                    if (transaccion.TipoTransaccion == "AperturaCaja")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var paquete = JsonSerializer.Deserialize<PaqueteAperturaOffline>(transaccion.DatosJson, opciones);

                        if (paquete != null && paquete.DatosApertura != null)
                        {
                            // PREVENCIÓN DE BUCLE: Si la caja o el usuario son falsos/offline
                            if (paquete.IdCaja <= 0 || paquete.DatosApertura.IdUsuario <= 0)
                            {
                                exitoAlEnviar = true; // Lo descartamos
                            }
                            else
                            {
                                var urlCore = $"https://localhost:56678/api/Cajas/{paquete.IdCaja}/abrir?idUsuario={paquete.DatosApertura.IdUsuario}&saldoInicial={paquete.DatosApertura.SaldoInicial}";
                                var respuesta = await cliente.PostAsync(urlCore, null);

                                if (respuesta.IsSuccessStatusCode)
                                {
                                    exitoAlEnviar = true;
                                }
                                else
                                {
                                    var mensajeError = await respuesta.Content.ReadAsStringAsync();
                                    if (mensajeError.Contains("ya está abierta") || mensajeError.Contains("FOREIGN KEY"))
                                    {
                                        exitoAlEnviar = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "CierreCaja")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var paquete = JsonSerializer.Deserialize<PaqueteCierreOffline>(transaccion.DatosJson, opciones);

                        if (paquete != null && paquete.DatosApertura != null) // Usando DatosApertura según tu comentario
                        {
                            // PREVENCIÓN DE BUCLE: Si la caja o el usuario son falsos/offline
                            if (paquete.IdCaja <= 0 || paquete.DatosApertura.IdUsuario <= 0)
                            {
                                exitoAlEnviar = true; // Lo descartamos
                            }
                            else
                            {
                                var urlCore = $"https://localhost:56678/api/Cajas/{paquete.IdCaja}/cerrar?idUsuario={paquete.DatosApertura.IdUsuario}&saldoFinal={paquete.DatosApertura.SaldoFinal}";
                                var respuesta = await cliente.PostAsync(urlCore, null);

                                if (respuesta.IsSuccessStatusCode)
                                {
                                    exitoAlEnviar = true;
                                }
                                else
                                {
                                    var mensajeError = await respuesta.Content.ReadAsStringAsync();
                                    if (mensajeError.Contains("no está abierta") || mensajeError.Contains("FOREIGN KEY"))
                                    {
                                        exitoAlEnviar = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "NuevoCliente")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var paquete = JsonSerializer.Deserialize<DatosClienteOffline>(transaccion.DatosJson, opciones);

                        if (paquete != null)
                        {
                            var urlCore = "https://localhost:56678/api/Clientes";
                            var jsonContent = new StringContent(JsonSerializer.Serialize(paquete), Encoding.UTF8, "application/json");
                            var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                            if (respuesta.IsSuccessStatusCode)
                            {
                                exitoAlEnviar = true;
                            }
                            else
                            {
                                // Filtro de errores de negocio (Ej. Si la cédula ya existe)
                                var mensajeError = await respuesta.Content.ReadAsStringAsync();
                                if (mensajeError.Contains("ya existe") || mensajeError.Contains("FOREIGN KEY"))
                                {
                                    exitoAlEnviar = true;
                                }
                            }
                        }
                    }

                    else if (transaccion.TipoTransaccion == "NuevoProducto")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var paquete = JsonSerializer.Deserialize<DatosProductoOffline>(transaccion.DatosJson, opciones);

                        if (paquete != null)
                        {
                            var urlCore = "https://localhost:56678/api/Productos";
                            var jsonContent = new StringContent(JsonSerializer.Serialize(paquete), Encoding.UTF8, "application/json");
                            var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                            if (respuesta.IsSuccessStatusCode) exitoAlEnviar = true;
                            else
                            {
                                var mensajeError = await respuesta.Content.ReadAsStringAsync();
                                if (mensajeError.Contains("FOREIGN KEY")) exitoAlEnviar = true; // Cola envenenada
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "ActualizarPrecioProducto")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var paquete = JsonSerializer.Deserialize<DatosPatchPrecioOffline>(transaccion.DatosJson, opciones);

                        if (paquete != null)
                        {
                            if (paquete.IdProducto <= 0)
                            {
                                exitoAlEnviar = true; // Lo marcamos como "éxito" para que se borre de SQLite
                            }
                            else
                            {
                                var urlCore = $"https://localhost:56678/api/Productos/{paquete.IdProducto}/precio?nuevoPrecio={paquete.NuevoPrecio}";
                                if (paquete.NuevoCosto.HasValue) urlCore += $"&nuevoCosto={paquete.NuevoCosto.Value}";

                                var respuesta = await cliente.PatchAsync(urlCore, null);

                                // Si fue exitoso o el CORE dice "No existe" (404), la borramos para evitar bucles
                                if (respuesta.IsSuccessStatusCode || respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                                {
                                    exitoAlEnviar = true;
                                }
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "OrdenCompleta")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var carrito = JsonSerializer.Deserialize<OrdenCompletaDto>(transaccion.DatosJson, opciones);

                        if (carrito != null)
                        {
                            // PREVENCIÓN DE BUCLE: Si se facturó a un cliente offline (ID <= 0)
                            // en un sistema real esperaríamos a que el cliente se sincronice primero.
                            // Aquí lo descartamos si no es válido para proteger la BD central.
                            if (carrito.IdCliente <= 0 || carrito.IdCaja <= 0)
                            {
                                exitoAlEnviar = true;
                            }
                            else
                            {
                                // Replicamos la misma lógica del controlador
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

                                var resOrden = await cliente.PostAsync(urlCrearOrden, new StringContent(JsonSerializer.Serialize(datosOrden), Encoding.UTF8, "application/json"));

                                if (resOrden.IsSuccessStatusCode)
                                {
                                    var idOrdenStr = await resOrden.Content.ReadAsStringAsync();
                                    var match = System.Text.RegularExpressions.Regex.Match(idOrdenStr, @"\d+");

                                    if (match.Success)
                                    {
                                        int idOrdenNueva = int.Parse(match.Value);

                                        // VARIABLE DE CONTROL PARA EL ROLLBACK
                                        bool errorEnDetalles = false;

                                        // Metemos productos
                                        foreach (var prod in carrito.Productos)
                                        {
                                            var datosProd = new { idOrden = idOrdenNueva, idProducto = prod.IdProducto, cantidad = prod.Cantidad };
                                            var jsonProd = new StringContent(JsonSerializer.Serialize(datosProd), Encoding.UTF8, "application/json");
                                            var resProd = await cliente.PostAsync("https://localhost:56678/api/Ordenes/producto", jsonProd);

                                            if (!resProd.IsSuccessStatusCode) errorEnDetalles = true; // Atrapamos el error
                                        }

                                        // Metemos servicios
                                        foreach (var serv in carrito.Servicios)
                                        {
                                            var datosServ = new { idOrden = idOrdenNueva, idServicio = serv.IdServicio, idEmpleadoTecnico = serv.IdEmpleadoTecnico, precioCobrado = serv.PrecioCobrado, descripcionTrabajo = serv.DescripcionTrabajo };
                                            var jsonServ = new StringContent(JsonSerializer.Serialize(datosServ), Encoding.UTF8, "application/json");
                                            var resServ = await cliente.PostAsync("https://localhost:56678/api/Ordenes/servicio", jsonServ);

                                            if (!resServ.IsSuccessStatusCode) errorEnDetalles = true; // Atrapamos el error
                                        }

                                        // ROLLBACK COMPENSATORIO
                                        if (errorEnDetalles)
                                        {
                                            // Falló por falta de stock o error de negocio. Cancelamos la orden huérfana en Azure.
                                            await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrdenNueva}/cancelar", null);

                                            // La sacamos de la cola local para no generar un bucle de órdenes canceladas
                                            exitoAlEnviar = true;
                                        }
                                        else
                                        {
                                            // 4 y 5. CONFIRMACIÓN Y FACTURACIÓN INTELIGENTE OFFLINE
                                            if (carrito.TipoOrden == "Factura")
                                            {
                                                // Primero confirmamos
                                                await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrdenNueva}/confirmar", null);

                                                // Luego facturamos
                                                await cliente.PostAsync($"https://localhost:56678/api/Facturas?idOrden={idOrdenNueva}&idEmpleado={carrito.IdEmpleado}&idCaja={carrito.IdCaja}&tipoFactura=Consumidor_Final", null);
                                            }

                                            exitoAlEnviar = true; // ¡Mega-proceso completado exitosamente!
                                        }
                                    }
                                }
                                else
                                {
                                    var mensajeError = await resOrden.Content.ReadAsStringAsync();
                                    if (mensajeError.Contains("FOREIGN KEY")) exitoAlEnviar = true;
                                }
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "ConfirmarOrden" || transaccion.TipoTransaccion == "CancelarOrden")
                    {
                        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        using JsonDocument doc = JsonDocument.Parse(transaccion.DatosJson);
                        int idOrden = doc.RootElement.GetProperty("IdOrden").GetInt32();

                        string accion = transaccion.TipoTransaccion == "ConfirmarOrden" ? "confirmar" : "cancelar";
                        var urlCore = $"https://localhost:56678/api/Ordenes/{idOrden}/{accion}";

                        var respuesta = await cliente.PostAsync(urlCore, null);

                        if (respuesta.IsSuccessStatusCode || respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            exitoAlEnviar = true;
                        }
                    }

                    else if (transaccion.TipoTransaccion == "NuevoEmpleado")
                    {
                        var urlCore = "https://localhost:56678/api/Empleados";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else
                        {
                            var mensajeError = await respuesta.Content.ReadAsStringAsync();
                            if (mensajeError.Contains("ya existe") || mensajeError.Contains("FOREIGN KEY"))
                            {
                                exitoAlEnviar = true;
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "NuevoVehiculo")
                    {
                        var urlCore = "https://localhost:56678/api/Vehiculos";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else
                        {
                            if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest || await respuesta.Content.ReadAsStringAsync() is var msg && msg.Contains("FOREIGN KEY"))
                            {
                                exitoAlEnviar = true;
                            }
                        }
                    }

                    else if (transaccion.TipoTransaccion == "AjustarStockProducto")
                    {
                        var urlCore = "https://localhost:56678/api/Productos/ajustar-stock";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            exitoAlEnviar = true;
                        }
                    }

                    else if (transaccion.TipoTransaccion == "AprobarCotizacion")
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(transaccion.DatosJson);
                        int idOrden = payload.GetProperty("IdOrden").GetInt32();

                        var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrden}/aprobar", new StringContent(""));

                        if (res.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                            _logger.LogInformation($"Sincronización Exitosa: Cotización {idOrden} aprobada.");
                        }
                        else if ((int)res.StatusCode >= 400)
                        {
                            // Si da error de negocio (400, 404, 500 de validación), lo matamos para no hacer bucle.
                            exitoAlEnviar = true;
                            _logger.LogWarning($"Descartada: El CORE rechazó aprobar la orden {idOrden}.");
                        }
                    }
                    else if (transaccion.TipoTransaccion == "ConvertirAFactura")
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(transaccion.DatosJson);
                        int idOrden = payload.GetProperty("IdOrden").GetInt32();

                        var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrden}/convertir", new StringContent(""));

                        if (res.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                            _logger.LogInformation($"Sincronización Exitosa: Cotización {idOrden} convertida a factura.");
                        }
                        else if ((int)res.StatusCode >= 400)
                        {
                            exitoAlEnviar = true;
                        }
                    }
                    else if (transaccion.TipoTransaccion == "AnularOrden")
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(transaccion.DatosJson);
                        int idOrden = payload.GetProperty("IdOrden").GetInt32();

                        // Mandamos el body vacío como solicitaste
                        var res = await cliente.PostAsync($"https://localhost:56678/api/Ordenes/{idOrden}/anular", new StringContent(""));

                        if (res.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                            _logger.LogInformation($"Sincronización Exitosa: Orden {idOrden} anulada.");
                        }
                        else if ((int)res.StatusCode >= 400)
                        {
                            exitoAlEnviar = true;
                        }
                    }
                    else if (transaccion.TipoTransaccion == "AsignarEmpleadoOrden")
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(transaccion.DatosJson);
                        int idOrden = payload.GetProperty("IdOrden").GetInt32();
                        int idEmpleado = payload.GetProperty("IdEmpleado").GetInt32();

                        // CORRECCIÓN DEL JSON: Ahora enviamos { "idEmpleado": X }
                        var jsonContent = JsonSerializer.Serialize(new { idEmpleado = idEmpleado });
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var res = await cliente.PatchAsync($"https://localhost:56678/api/Ordenes/{idOrden}/asignar-empleado", content);

                        if (res.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                            _logger.LogInformation($"Sincronización Exitosa: Empleado {idEmpleado} asignado a orden {idOrden}.");
                        }
                        else if ((int)res.StatusCode >= 400)
                        {
                            exitoAlEnviar = true;
                        }
                    }
                    else if (transaccion.TipoTransaccion == "ActualizarPreciosLote")
                    {
                        var url = "https://localhost:56678/api/Productos/actualizar-precios";

                        var jsonEnvuelto = $"{{\"precios\": {transaccion.DatosJson}}}";
                        var content = new StringContent(jsonEnvuelto, Encoding.UTF8, "application/json");

                        var res = await cliente.PostAsync(url, content);
                        if (res.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                            _logger.LogInformation($"Sincronización Exitosa: Lote de precios actualizado en el CORE.");
                        }
                        else if ((int)res.StatusCode >= 400)
                        {
                            exitoAlEnviar = true;
                        }
                    }


                    else if (transaccion.TipoTransaccion == "NuevoEmpleado")
                    {
                        var urlCore = "https://localhost:56678/api/Empleados";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else
                        {
                            var mensajeError = await respuesta.Content.ReadAsStringAsync();
                            if (mensajeError.Contains("ya existe") || mensajeError.Contains("FOREIGN KEY"))
                            {
                                exitoAlEnviar = true;
                            }
                        }
                    }
                    else if (transaccion.TipoTransaccion == "NuevoVehiculo")
                    {
                        var urlCore = "https://localhost:56678/api/Vehiculos";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else
                        {
                            if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest || await respuesta.Content.ReadAsStringAsync() is var msg && msg.Contains("FOREIGN KEY"))
                            {
                                exitoAlEnviar = true;
                            }
                        }
                    }

                    else if (transaccion.TipoTransaccion == "AjustarStockProducto")
                    {
                        var urlCore = "https://localhost:56678/api/Productos/ajustar-stock";
                        var jsonContent = new StringContent(transaccion.DatosJson, Encoding.UTF8, "application/json");

                        var respuesta = await cliente.PostAsync(urlCore, jsonContent);

                        if (respuesta.IsSuccessStatusCode)
                        {
                            exitoAlEnviar = true;
                        }
                        else if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            exitoAlEnviar = true;
                        }
                    }

                    if (exitoAlEnviar)
                    {
                        context.TransaccionesPendientes.Remove(transaccion);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

            await context.SaveChangesAsync();
        }

        // Clases auxiliares para leer el JSON guardado
        private class PaqueteAperturaOffline
        {
            public int IdCaja { get; set; }
            public DatosApertura? DatosApertura { get; set; } 
        }

        private class DatosApertura
        {
            public int IdUsuario { get; set; }
            public decimal SaldoInicial { get; set; }
        }

        private class PaqueteCierreOffline
        {
            public int IdCaja { get; set; }
            public DatosCierre? DatosApertura { get; set; } // Lo llamamos DatosApertura en el método genérico GuardarEnColaOffline, no pasa nada
        }

        private class DatosCierre
        {
            public int IdUsuario { get; set; }
            public decimal SaldoFinal { get; set; }
        }
        private class DatosClienteOffline
        {
            public int IdSector { get; set; }
            public int IdMunicipio { get; set; }
            public int IdProvincia { get; set; }
            public string NombreCliente { get; set; } = string.Empty;
            public string ApellidoCliente { get; set; } = string.Empty;
            public string Rnc { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
        }

        private class DatosProductoOffline
        {
            public int IdCategoria { get; set; }
            public int IdTipoTrans { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public decimal PrecioUnitario { get; set; }
            public decimal CostoUnitario { get; set; }
            public string Marca { get; set; } = string.Empty;
            public int StockInicial { get; set; }
        }

        private class DatosPatchPrecioOffline
        {
            public int IdProducto { get; set; }
            public decimal NuevoPrecio { get; set; }
            public decimal? NuevoCosto { get; set; }
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