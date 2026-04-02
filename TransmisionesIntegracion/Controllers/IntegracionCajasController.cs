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