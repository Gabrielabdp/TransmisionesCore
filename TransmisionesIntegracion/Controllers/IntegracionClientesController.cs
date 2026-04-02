using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/clientes")]
    [ApiController]
    public class IntegracionClientesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionClientesController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerClientes([FromQuery] string? buscar = null)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                // Si mandaron un texto a buscar, lo pegamos en la URL, si no, lo dejamos vacío
                var urlCore = "https://localhost:56678/api/Clientes" +
                              (string.IsNullOrWhiteSpace(buscar) ? "" : $"?buscar={buscar}");

                var respuestaCore = await cliente.GetAsync(urlCore);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    var jsonCore = await respuestaCore.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var clientesCore = JsonSerializer.Deserialize<List<ClienteCoreDto>>(jsonCore, opciones);

                    if (clientesCore != null)
                    {
                        // Solo actualizamos el caché si trajimos TODOS los clientes (sin filtro)
                        // para no borrar el caché accidentalmente con una búsqueda pequeña
                        if (string.IsNullOrWhiteSpace(buscar))
                        {
                            _context.ClientesCache.RemoveRange(_context.ClientesCache);
                            foreach (var c in clientesCore)
                            {
                                _context.ClientesCache.Add(new ClienteCache
                                {
                                    Id = c.Id_cliente,
                                    Nombre = c.Nombre_cliente,
                                    Apellido = c.Apellido_cliente,
                                    Documento = !string.IsNullOrEmpty(c.RNC_cliente) ? c.RNC_cliente : c.Cedula_cliente ?? "",
                                    Telefono = c.Telefono_cliente ?? ""
                                });
                            }
                            await _context.SaveChangesAsync();
                        }
                    }

                    return Content(jsonCore, "application/json");
                }
                else
                {
                    return await ResponderDesdeCache(buscar);
                }
            }
            catch (Exception)
            {
                return await ResponderDesdeCache(buscar);
            }
        }

        

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerClientePorId(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);
                var urlCore = $"https://localhost:56678/api/Clientes/{id}";

                var respuestaCore = await cliente.GetAsync(urlCore);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    var jsonCore = await respuestaCore.Content.ReadAsStringAsync();
                    return Content(jsonCore, "application/json");
                }
                else
                {
                    // Búsqueda Offline por ID
                    var clienteLocal = _context.ClientesCache.FirstOrDefault(c => c.Id == id);
                    if (clienteLocal != null) return Ok(new { modoOffline = true, datos = clienteLocal });
                    return NotFound(new { mensaje = "Cliente no encontrado en el caché local." });
                }
            }
            catch (Exception)
            {
                // Búsqueda Offline por ID
                var clienteLocal = _context.ClientesCache.FirstOrDefault(c => c.Id == id);
                if (clienteLocal != null) return Ok(new { modoOffline = true, datos = clienteLocal });
                return NotFound(new { mensaje = "Cliente no encontrado en el caché local." });
            }
        }

        private async Task<IActionResult> ResponderDesdeCache(string? buscar)
        {
            var consulta = _context.ClientesCache.AsQueryable();

            // Filtro Offline
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(c => c.Nombre.Contains(buscar) ||
                                               c.Apellido.Contains(buscar) ||
                                               c.Documento.Contains(buscar));
            }

            var clientesLocales = consulta.ToList();

            if (clientesLocales.Any())
            {
                return Ok(new { modoOffline = true, mensaje = "Mostrando directorio local de clientes.", datos = clientesLocales });
            }
            return StatusCode(503, "Sistema central caído. No hay clientes en caché que coincidan.");
        }


        [HttpPost]
        public async Task<IActionResult> RegistrarCliente([FromBody] CrearClienteIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var urlCore = "https://localhost:56678/api/Clientes";

                var jsonContent = new StringContent(JsonSerializer.Serialize(peticion), Encoding.UTF8, "application/json");
                var respuestaCore = await cliente.PostAsync(urlCore, jsonContent);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    return Ok(new { exito = true, mensaje = "Cliente registrado en el sistema central exitosamente." });
                }
                else
                {
                    return await GuardarEnColaOffline("NuevoCliente", peticion);
                }
            }
            catch (Exception)
            {
                return await GuardarEnColaOffline("NuevoCliente", peticion);
            }
        }

        // Cambiamos 'object' por el DTO específico para poder leer sus propiedades
        private async Task<IActionResult> GuardarEnColaOffline(string tipo, CrearClienteIntegracionDto datos)
        {
            // 1. Guardar en el Buzón (Para que el cartero lo envíe luego)
            var jsonEmpacado = JsonSerializer.Serialize(datos);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = tipo,
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now,
                Sincronizado = false
            });

            // 2. MAGIA: Guardar en la Vitrina (Caché Optimista)
            // Buscamos el ID más bajito y le restamos 1 para crear IDs falsos (-1, -2, -3...)
            var idFalso = _context.ClientesCache.Any() ? _context.ClientesCache.Min(c => c.Id) - 1 : -1;
            if (idFalso >= 0) idFalso = -1; // Seguro por si acaso

            _context.ClientesCache.Add(new ClienteCache
            {
                Id = idFalso,
                Nombre = datos.NombreCliente,
                Apellido = datos.ApellidoCliente,
                Documento = !string.IsNullOrEmpty(datos.Rnc) ? datos.Rnc : datos.Cedula ?? "Pendiente",
                Telefono = datos.Telefono ?? ""
            });

            await _context.SaveChangesAsync();

            return Ok(new { modoOffline = true, exito = true, mensaje = "Sistema desconectado. Cliente guardado en cola y visible en el directorio local." });
        }
    }
    public class ClienteCoreDto
    {
        public int Id_cliente { get; set; }
        public string Nombre_cliente { get; set; } = string.Empty;
        public string Apellido_cliente { get; set; } = string.Empty;
        public string? RNC_cliente { get; set; }
        public string? Cedula_cliente { get; set; }
        public string? Telefono_cliente { get; set; }
    }
    public class CrearClienteIntegracionDto
    {
        public int IdSector { get; set; }
        public int IdMunicipio { get; set; }
        public int IdProvincia { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string? Rnc { get; set; }
        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}