using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TransmisionesIntegracion.Services;

namespace TransmisionesIntegracion.Controllers;

[ApiController]
[Route("api/integracion/facturas")]
public class IntegracionFacturasController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly EmailService _email;
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public IntegracionFacturasController(IHttpClientFactory httpFactory, EmailService email)
    {
        _httpFactory = httpFactory;
        _email = email;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerFactura(int id)
    {
        try
        {
            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            var resp = await http.GetAsync($"https://localhost:56678/api/Facturas/{id}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            return StatusCode((int)resp.StatusCode, new { mensaje = "Error al obtener factura del sistema central." });
        }
        catch
        {
            return StatusCode(503, new { mensaje = "Factura no disponible en modo offline. Verifica tu conexión." });
        }
    }

    [HttpPost("{id}/enviar-email")]
    public async Task<IActionResult> EnviarEmail(int id)
    {
        try
        {
            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(10);

            var respFactura = await http.GetAsync($"https://localhost:56678/api/Facturas/{id}");
            if (!respFactura.IsSuccessStatusCode)
                return StatusCode(503, new { mensaje = "No se pudo obtener la factura. Verifica conexión." });

            var jsonFactura = await respFactura.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonFactura);
            var root = doc.RootElement;

            // Leer campos — los nombres reales según el endpoint GET /api/Facturas/{id}
            // son: Numero_factura, Fecha_factura, SubTotal, ITBIS, Total, CorreoCliente, NombreCliente
            var correo = root.TryGetProperty("correoCliente", out var correoEl) ? correoEl.GetString()
                       : root.TryGetProperty("CorreoCliente", out var correoEl2) ? correoEl2.GetString()
                       : null;

            var nombre = root.TryGetProperty("nombreCliente", out var nombreEl) ? nombreEl.GetString()
                       : root.TryGetProperty("NombreCliente", out var nombreEl2) ? nombreEl2.GetString()
                       : "Cliente";

            var numero = root.TryGetProperty("numero_factura", out var numEl) ? numEl.GetString()
                       : root.TryGetProperty("Numero_factura", out var numEl2) ? numEl2.GetString()
                       : id.ToString();

            if (string.IsNullOrWhiteSpace(correo))
                return BadRequest(new { mensaje = "El cliente no tiene correo registrado." });

            // Parsear subtotal, ITBIS, total (pueden ser SubTotal o subtotal)
            decimal subtotal = TryGetDecimal(root, "SubTotal", "subTotal", "subtotal");
            decimal itbis = TryGetDecimal(root, "ITBIS", "itbis");
            decimal total = TryGetDecimal(root, "Total", "total");
            DateTime fecha = root.TryGetProperty("Fecha_factura", out var fechaEl) ? fechaEl.GetDateTime()
                           : root.TryGetProperty("fecha_emision", out var fechaEl2) ? fechaEl2.GetDateTime()
                           : DateTime.Now;

            // Parsear detalles
            var detalles = new List<DetalleEmailData>();
            if (root.TryGetProperty("Detalles", out var detallesEl) || root.TryGetProperty("detalles", out detallesEl))
            {
                foreach (var d in detallesEl.EnumerateArray())
                {
                    detalles.Add(new DetalleEmailData
                    {
                        Descripcion = d.TryGetProperty("Descripcion", out var descEl) ? descEl.GetString() ?? ""
                                    : d.TryGetProperty("descripcion", out var descEl2) ? descEl2.GetString() ?? "" : "",
                        Cantidad = d.TryGetProperty("Cantidad", out var cantEl) ? cantEl.GetInt32()
                                 : d.TryGetProperty("cantidad", out var cantEl2) ? cantEl2.GetInt32() : 1,
                        PrecioUnitario = TryGetDecimalEl(d, "PrecioUnitario", "precioUnitario"),
                        Subtotal = TryGetDecimalEl(d, "Subtotal", "subtotal")
                    });
                }
            }

            var datos = new FacturaEmailData
            {
                NumeroFactura = numero ?? id.ToString(),
                FechaEmision = fecha,
                Subtotal = subtotal,
                ITBIS = itbis,
                Total = total,
                Detalles = detalles
            };

            await _email.EnviarFacturaAsync(correo, nombre ?? "Cliente", datos);
            return Ok(new { exito = true, mensaje = $"Factura enviada a {correo}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al enviar el correo: {ex.Message}" });
        }
    }

    private static decimal TryGetDecimal(JsonElement root, params string[] names)
    {
        foreach (var name in names)
            if (root.TryGetProperty(name, out var el) && el.TryGetDecimal(out var val))
                return val;
        return 0m;
    }

    private static decimal TryGetDecimalEl(JsonElement el, params string[] names)
    {
        foreach (var name in names)
            if (el.TryGetProperty(name, out var prop) && prop.TryGetDecimal(out var val))
                return val;
        return 0m;
    }
}
