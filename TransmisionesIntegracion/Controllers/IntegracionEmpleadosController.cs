using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/empleados")]
    [ApiController]
    public class IntegracionEmpleadosController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionEmpleadosController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEmpleados()
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var respuesta = await cliente.GetAsync("https://localhost:56678/api/Empleados");

                if (respuesta.IsSuccessStatusCode)
                {
                    return Content(await respuesta.Content.ReadAsStringAsync(), "application/json");
                }
                return await ResponderDesdeCache();
            }
            catch
            {
                return await ResponderDesdeCache();
            }
        }

        private async Task<IActionResult> ResponderDesdeCache()
        {
            var locales = await _context.EmpleadosCache.Where(e => e.Activo).ToListAsync();
            return Ok(new { modoOffline = true, datos = locales });
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEmpleado([FromBody] CrearEmpleadoIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var jsonContent = new StringContent(JsonSerializer.Serialize(peticion), Encoding.UTF8, "application/json");
                var respuesta = await cliente.PostAsync("https://localhost:56678/api/Empleados", jsonContent);

                if (respuesta.IsSuccessStatusCode)
                {
                    return Ok(new { exito = true, mensaje = "Empleado registrado en el sistema central." });
                }

                // CORRECCIÓN: Si el CORE está encendido pero los datos están mal (400 Bad Request), 
                // se lo informamos al FrontEnd y NO lo guardamos en la cola local.
                if (respuesta.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorBody = await respuesta.Content.ReadAsStringAsync();
                    return BadRequest(new
                    {
                        exito = false,
                        mensaje = "El CORE rechazó los datos por errores de validación.",
                        detalle = errorBody
                    });
                }

                // Si es Error 500 (Caída del servidor) o cualquier otro error extraño, lo encolamos.
                return await GuardarEnColaOffline(peticion);
            }
            catch
            {
                // Si hay un Timeout o Exception (No hay internet), lo encolamos.
                return await GuardarEnColaOffline(peticion);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEmpleadoPorId(int id)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(10);
                var urlCore = $"https://localhost:56678/api/Empleados/{id}";

                var respuestaCore = await cliente.GetAsync(urlCore);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    var jsonCore = await respuestaCore.Content.ReadAsStringAsync();
                    return Content(jsonCore, "application/json");
                }
                else if (respuestaCore.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new { mensaje = "El empleado no existe en el sistema central." });
                }

                return await BuscarEmpleadoEnCache(id);
            }
            catch (Exception)
            {
                return await BuscarEmpleadoEnCache(id);
            }
        }

        private async Task<IActionResult> BuscarEmpleadoEnCache(int id)
        {
            var empleadoLocal = await _context.EmpleadosCache.FirstOrDefaultAsync(e => e.Id == id);

            if (empleadoLocal != null)
            {
                return Ok(new
                {
                    modoOffline = true,
                    mensaje = "Mostrando datos del empleado desde el caché local.",
                    datos = empleadoLocal
                });
            }

            return NotFound(new { mensaje = "(Offline) El empleado no se encuentra en el caché local." });
        }

        private async Task<IActionResult> GuardarEnColaOffline(CrearEmpleadoIntegracionDto datos)
        {
            // 1. Guardar en el Buzón de salida (Para que el Worker lo suba luego)
            var jsonEmpacado = JsonSerializer.Serialize(datos);
            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = "NuevoEmpleado",
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now,
                Sincronizado = false
            });

            // 2. Caché Optimista: Guardar en la vitrina local para que el GET lo vea INMEDIATAMENTE
            var idFalso = _context.EmpleadosCache.Any() ? _context.EmpleadosCache.Min(e => e.Id) - 1 : -1;
            if (idFalso >= 0) idFalso = -1; // Seguro por si acaso

            _context.EmpleadosCache.Add(new EmpleadoCache
            {
                Id = idFalso,
                NombreCompleto = $"{datos.Nombre} {datos.Apellido} (Pendiente)", // Le ponemos la etiqueta para que el cajero sepa que es local
                Cedula = datos.Cedula,
                Activo = datos.Activo,

                // Si enviaron datos de usuario, los guardamos temporalmente para que pueda hacer Login offline
                IdUsuario = datos.Usuario != null ? idFalso : null,
                UsuarioAcceso = datos.Usuario != null ? datos.Usuario.Nombre_usuario : "",
                PasswordHash = datos.Usuario != null ? datos.Usuario.Contrasena : "",
                Rol = datos.Usuario != null ? datos.Usuario.Rol : ""
            });

            await _context.SaveChangesAsync();
            return Ok(new { modoOffline = true, exito = true, mensaje = "Empleado guardado localmente y visible en caché. Se sincronizará cuando regrese la conexión." });
        }
    }

    // DTO estricto para limpiar el Swagger y evitar envíos masivos de objetos anidados
    public class CrearEmpleadoIntegracionDto
    {
        public int Id_sucursal { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Fecha_ingreso { get; set; }
        public bool Activo { get; set; }

        public CrearUsuarioAnidadoDto? Usuario { get; set; }
    }

    public class CrearUsuarioAnidadoDto
    {
        public string Nombre_usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}