using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/vehiculos")]
    [ApiController]
    public class IntegracionVehiculosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;
        private readonly ILogger<IntegracionVehiculosController> _logger;

        public IntegracionVehiculosController(IHttpClientFactory httpClientFactory, IntegracionDbContext context, ILogger<IntegracionVehiculosController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        [HttpGet("matricula/{matricula}")]
        public async Task<IActionResult> ObtenerPorMatricula(string matricula)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(5);
                var respuesta = await cliente.GetAsync($"https://localhost:56678/api/Vehiculos/matricula/{matricula}");

                if (respuesta.IsSuccessStatusCode)
                    return Content(await respuesta.Content.ReadAsStringAsync(), "application/json");

                return await BuscarMatriculaOffline(matricula);
            }
            catch
            {
                return await BuscarMatriculaOffline(matricula);
            }
        }

        [HttpGet("{matricula}/historial")]
        public async Task<IActionResult> ObtenerHistorialClinico(string matricula)
        {
            return await EjecutarConsultaProxy($"https://localhost:56678/api/Vehiculos/{matricula}/historial");
        }

        [HttpGet("cliente/{idCliente}")]
        public async Task<IActionResult> ObtenerPorCliente(int idCliente)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(5);
                var respuesta = await cliente.GetAsync($"https://localhost:56678/api/Vehiculos/cliente/{idCliente}");

                if (respuesta.IsSuccessStatusCode)
                {
                    // Si consultamos los de un cliente, aprovechamos para guardarlos en la caché local
                    var json = await respuesta.Content.ReadAsStringAsync();
                    await ActualizarCacheLocalVehiculos(json);
                    return Content(json, "application/json");
                }
                return await BuscarPorClienteOffline(idCliente);
            }
            catch
            {
                return await BuscarPorClienteOffline(idCliente);
            }
        }



        [HttpPost]
        public async Task<IActionResult> RegistrarVehiculo([FromBody] VehiculoIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(5);
                var jsonContent = new StringContent(JsonSerializer.Serialize(peticion), Encoding.UTF8, "application/json");
                var respuesta = await cliente.PostAsync("https://localhost:56678/api/Vehiculos", jsonContent);

                if (respuesta.IsSuccessStatusCode)
                    return Ok(new { exito = true, mensaje = "Vehículo registrado en Azure." });

                if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    return BadRequest(new { exito = false, mensaje = "Error de validación en Azure.", detalle = await respuesta.Content.ReadAsStringAsync() });

                return await GuardarEnColaOffline(peticion);
            }
            catch
            {
                return await GuardarEnColaOffline(peticion);
            }
        }

        // --- MÉTODOS OFFLINE ---

        private async Task<IActionResult> BuscarMatriculaOffline(string matricula)
        {
            var vehiculo = await _context.VehiculosCache.FirstOrDefaultAsync(v => v.Matricula == matricula);
            if (vehiculo != null) return Ok(new { modoOffline = true, datos = vehiculo });
            return NotFound(new { mensaje = "(Offline) Vehículo no encontrado." });
        }

        private async Task<IActionResult> BuscarPorClienteOffline(int idCliente)
        {
            var vehiculos = await _context.VehiculosCache.Where(v => v.IdCliente == idCliente).ToListAsync();
            return Ok(new { modoOffline = true, datos = vehiculos });
        }

        private async Task<IActionResult> GuardarEnColaOffline(VehiculoIntegracionDto datos)
        {
            // 1. A la cola para subirlo luego
            var jsonEmpacado = JsonSerializer.Serialize(datos);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = "NuevoVehiculo",
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now
            });

            // 2. Caché Optimista: Lo ponemos en la base de datos local al instante
            // para que si el cajero lo busca 5 segundos después, lo encuentre.
            var vehiculoExistente = await _context.VehiculosCache.FirstOrDefaultAsync(v => v.Matricula == datos.Matricula);
            if (vehiculoExistente == null)
            {
                _context.VehiculosCache.Add(new VehiculoCache
                {
                    Matricula = datos.Matricula,
                    IdCliente = datos.Id_cliente,
                    IdTipoTrans = datos.Id_tipo_trans,
                    Marca = datos.Marca,
                    Modelo = datos.Modelo,
                    Anio = datos.Anio,
                    Color = datos.Color,
                    UltimaActualizacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = "Vehículo guardado en el taller (Desconectado)." });
        }

        private async Task ActualizarCacheLocalVehiculos(string jsonCore)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonCore);
                foreach (var v in doc.RootElement.EnumerateArray())
                {
                    string matricula = v.GetProperty("matricula").GetString() ?? "";
                    var local = await _context.VehiculosCache.FirstOrDefaultAsync(x => x.Matricula == matricula);

                    if (local == null)
                    {
                        _context.VehiculosCache.Add(new VehiculoCache
                        {
                            Matricula = matricula,
                            IdCliente = v.GetProperty("id_cliente").GetInt32(),
                            IdTipoTrans = v.GetProperty("id_tipo_trans").GetInt32(),
                            Marca = v.GetProperty("marca").GetString() ?? "",
                            Modelo = v.GetProperty("modelo").GetString() ?? "",
                            Anio = v.GetProperty("anio").GetInt32(),
                            Color = v.GetProperty("color").GetString() ?? "",
                            UltimaActualizacion = DateTime.Now
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ocurrió un error al intentar guardar los vehículos en la caché local de SQLite.");
            }
        }

        private async Task<IActionResult> EjecutarConsultaProxy(string urlCore)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(8); // Tiempo corto para no bloquear la UI
                var respuesta = await cliente.GetAsync(urlCore);

                if (respuesta.IsSuccessStatusCode)
                {
                    var json = await respuesta.Content.ReadAsStringAsync();
                    return Content(json, "application/json");
                }
                return StatusCode((int)respuesta.StatusCode, "El servicio central reportó un error.");
            }
            catch (Exception)
            {
                return StatusCode(503, new
                {
                    error = "Offline",
                    mensaje = "Este reporte o consulta avanzada requiere conexión a internet y no está disponible en modo local."
                });
            }
        }
    }

    public class VehiculoIntegracionDto
    {
        public string Matricula { get; set; } = string.Empty;
        public int Id_cliente { get; set; }
        public int Id_tipo_trans { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
