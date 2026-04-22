using System.Net.Http.Json;
using System.Text.Json;
using TransmisionesWeb.Models;

namespace TransmisionesWeb.Services;

public class ApiIntegracionClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiIntegracionClient(HttpClient http) => _http = http;

    public async Task<EstadoSistemaDto?> GetEstadoAsync()
    {
        try { return await _http.GetFromJsonAsync<EstadoSistemaDto>("api/integracion/estado", _opts); }
        catch { return null; }
    }

    public async Task<ClienteDto?> BuscarClientePorDocumentoAsync(string documento)
    {
        try
        {
            var resp = await _http.GetAsync($"api/integracion/clientes/buscar/{documento}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("datos", out var datos))
                return JsonSerializer.Deserialize<ClienteDto>(datos.GetRawText(), _opts);
            return JsonSerializer.Deserialize<ClienteDto>(json, _opts);
        }
        catch { return null; }
    }

    public async Task<bool> RegistrarClienteAsync(object datos)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/integracion/clientes", datos);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<OrdenDto>> GetOrdenesPorClienteAsync(int idCliente)
    {
        try
        {
            var resp = await _http.GetAsync($"api/integracion/ordenes?idCliente={idCliente}");
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var src = root.TryGetProperty("datos", out var d) ? d : root;
            return JsonSerializer.Deserialize<List<OrdenDto>>(src.GetRawText(), _opts) ?? new();
        }
        catch { return new(); }
    }

    public async Task<FacturaDto?> GetFacturaAsync(int id)
    {
        try
        {
            var resp = await _http.GetAsync($"api/integracion/facturas/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<FacturaDto>(_opts);
        }
        catch { return null; }
    }

    public async Task<(bool Exito, string Mensaje)> EnviarFacturaPorEmailAsync(int id)
    {
        try
        {
            var resp = await _http.PostAsync($"api/integracion/facturas/{id}/enviar-email", null);
            var msg = resp.IsSuccessStatusCode ? "Factura enviada al correo del cliente." : "No se pudo enviar el correo.";
            return (resp.IsSuccessStatusCode, msg);
        }
        catch { return (false, "Error de conexión al enviar el correo."); }
    }

    public async Task<LoginResponseDto?> LoginAsync(string usuario, string password)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/integracion/autenticacion/login",
                new { Usuario = usuario, Password = password });
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<LoginResponseDto>(_opts);
        }
        catch { return null; }
    }

    public async Task<bool> AbrirCajaAsync(int idCaja, int idUsuario, decimal saldoInicial)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/integracion/cajas/{idCaja}/abrir",
                new { IdUsuario = idUsuario, SaldoInicial = saldoInicial });
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> CerrarCajaAsync(int idCaja, int idUsuario, decimal saldoFinal)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/integracion/cajas/{idCaja}/cerrar",
                new { IdUsuario = idUsuario, SaldoFinal = saldoFinal });
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<EstadoCajaDto?> GetEstadoCajaAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<EstadoCajaDto>($"api/integracion/cajas/{id}/estado-actual", _opts); }
        catch { return null; }
    }

    public async Task<ResumenCajaDiarioDto?> GetResumenHoyAsync()
    {
        try { return await _http.GetFromJsonAsync<ResumenCajaDiarioDto>("api/integracion/cajas/resumen-hoy", _opts); }
        catch { return null; }
    }

    public async Task<bool> RegistrarGastoAsync(int idCaja, string concepto, decimal monto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"api/integracion/cajas/{idCaja}/gasto",
                new { Concepto = concepto, Monto = monto });
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<ProductoDto>> BuscarProductosAsync(string buscar)
    {
        try
        {
            var resp = await _http.GetAsync($"api/integracion/productos?buscar={Uri.EscapeDataString(buscar)}");
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var src = root.TryGetProperty("datos", out var d) ? d : root;
            return JsonSerializer.Deserialize<List<ProductoDto>>(src.GetRawText(), _opts) ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<ServicioDto>> GetServiciosAsync()
    {
        try
        {
            var resp = await _http.GetAsync("api/integracion/servicios");
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var src = root.TryGetProperty("datos", out var d) ? d : root;
            return JsonSerializer.Deserialize<List<ServicioDto>>(src.GetRawText(), _opts) ?? new();
        }
        catch { return new(); }
    }

    public async Task<ProcesarOrdenResponseDto?> ProcesarOrdenAsync(object payload)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/integracion/ordenes/procesar", payload);
            if (!resp.IsSuccessStatusCode) return new(null, null, false, "Error al procesar la orden.");
            return await resp.Content.ReadFromJsonAsync<ProcesarOrdenResponseDto>(_opts);
        }
        catch { return new(null, null, false, "Error de conexión al procesar la orden."); }
    }
}
