using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TransmisionesIntegracion.Data;
using TransmisionesIntegracion.Models;

namespace TransmisionesIntegracion.Controllers
{
    [Route("api/integracion/cajas")]
    [ApiController]
    public class IntegracionCajasController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegracionDbContext _context;

        public IntegracionCajasController(IHttpClientFactory httpClientFactory, IntegracionDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpPost("{idCaja}/abrir")]
        public async Task<IActionResult> AbrirCaja(int idCaja, [FromBody] AbrirCajaIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var urlCore = $"https://localhost:56678/api/Cajas/{idCaja}/abrir?idUsuario={peticion.IdUsuario}&saldoInicial={peticion.SaldoInicial}";

                var datosCore = new { IdUsuario = peticion.IdUsuario, SaldoInicial = peticion.SaldoInicial };
                var jsonContent = new StringContent(JsonSerializer.Serialize(datosCore), Encoding.UTF8, "application/json");

                var respuestaCore = await cliente.PostAsync(urlCore, null);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    return Ok(new { exito = true, mensaje = "Caja abierta en el sistema central exitosamente." });
                }
                else
                {
                    return await GuardarEnColaOffline("AperturaCaja", idCaja, peticion);
                }
            }
            catch (Exception)
            {
                return await GuardarEnColaOffline("AperturaCaja", idCaja, peticion);
            }
        }

        [HttpPost("{idCaja}/cerrar")]
        public async Task<IActionResult> CerrarCaja(int idCaja, [FromBody] CerrarCajaIntegracionDto peticion)
        {
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var urlCore = $"https://localhost:56678/api/Cajas/{idCaja}/cerrar?idUsuario={peticion.IdUsuario}&saldoFinal={peticion.SaldoFinal}";

                var respuestaCore = await cliente.PostAsync(urlCore, null);

                if (respuestaCore.IsSuccessStatusCode)
                {
                    return Ok(new { exito = true, mensaje = "Caja cerrada en el sistema central exitosamente." });
                }
                else
                {
                    return await GuardarEnColaOffline("CierreCaja", idCaja, peticion);
                }
            }
            catch (Exception)
            {
                return await GuardarEnColaOffline("CierreCaja", idCaja, peticion);
            }
        }

        [HttpGet("resumen-hoy")]
        public async Task<IActionResult> ObtenerResumenVentasHoy()
        {
            return await EjecutarConsultaProxy("https://localhost:56678/api/Cajas/resumen-hoy");
        }

        private async Task<IActionResult> GuardarEnColaOffline(string tipo, int idCaja, object datos)
        {
            var paqueteCompleto = new
            {
                IdCaja = idCaja,
                DatosApertura = datos
            };

            var jsonEmpacado = JsonSerializer.Serialize(paqueteCompleto);

            _context.TransaccionesPendientes.Add(new TransaccionPendiente
            {
                TipoTransaccion = tipo,
                DatosJson = jsonEmpacado,
                FechaIntento = DateTime.Now,
                Sincronizado = false
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                modoOffline = true,
                exito = true,
                mensaje = "Sistema central desconectado. Apertura de caja guardada localmente. Se sincronizará al regresar la red."
            });
        }

        [HttpGet("{id}/estado-actual")]
        public async Task<IActionResult> ObtenerEstadoArqueo(int id)
        {
            return await EjecutarConsultaProxy($"https://localhost:56678/api/Cajas/{id}/estado-actual");
        }

        [HttpPost("{idCaja}/gasto")]
        public async Task<IActionResult> RegistrarGasto(int idCaja, [FromBody] GastoIntegracionDto peticion)
        {
            var payload = new { IdCaja = idCaja, Concepto = peticion.Concepto, Monto = peticion.Monto };
            _context.TransaccionesPendientes.Add(new TransmisionesIntegracion.Models.TransaccionPendiente
            {
                TipoTransaccion = "RegistrarGasto",
                DatosJson = System.Text.Json.JsonSerializer.Serialize(payload),
                FechaIntento = DateTime.Now,
                Sincronizado = false
            });
            await _context.SaveChangesAsync();
            return Ok(new { exito = true, mensaje = "Gasto registrado." });
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

    public class AbrirCajaIntegracionDto
    {
        public int IdUsuario { get; set; }
        public decimal SaldoInicial { get; set; }
    }
}
public class CerrarCajaIntegracionDto
{
    public int IdUsuario { get; set; }
    public decimal SaldoFinal { get; set; }
}

public class GastoIntegracionDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}