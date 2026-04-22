# TransmisionesWeb — Portal del Cliente y Terminal de Caja — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir `TransmisionesWeb` (Blazor Server) con portal de cliente y terminal de caja offline-first, integrando con `TransmisionesIntegracion` como único backend.

**Architecture:** Un solo proyecto Blazor Server con rutas `/` (portal cliente) y `/empleado/*` (caja). Toda comunicación va a `TransmisionesIntegracion` via un `HttpClient` tipado. Se agregan 4 endpoints nuevos a `TransmisionesIntegracion` y 1 endpoint a `TransmisionesAPI` antes de construir el frontend.

**Tech Stack:** .NET 8, Blazor Server (InteractiveServer), Bootstrap 5 + Bootstrap Icons, MailKit 4.x, Microsoft.Extensions.Http.Polly 8.x

---

## Mapa de archivos

### TransmisionesAPI (modificaciones)
| Archivo | Acción |
|---|---|
| `TransmisionesAPI/Controllers/FacturasController.cs` | Modificar — agregar `GET /{id}` |

### TransmisionesIntegracion (modificaciones)
| Archivo | Acción |
|---|---|
| `TransmisionesIntegracion/TransmisionesIntegracion.csproj` | Modificar — agregar MailKit |
| `TransmisionesIntegracion/appsettings.Development.json` | Modificar — sección Email |
| `TransmisionesIntegracion/Services/EmailService.cs` | Crear |
| `TransmisionesIntegracion/Controllers/IntegracionEstadoController.cs` | Crear |
| `TransmisionesIntegracion/Controllers/IntegracionCajasController.cs` | Modificar — agregar gasto |
| `TransmisionesIntegracion/Controllers/IntegracionFacturasController.cs` | Crear |
| `TransmisionesIntegracion/Program.cs` | Modificar — registrar EmailService |

### TransmisionesWeb (proyecto nuevo)
| Archivo | Acción |
|---|---|
| `TransmisionesWeb/TransmisionesWeb.csproj` | Crear |
| `TransmisionesSolution.sln` | Modificar — agregar proyecto |
| `TransmisionesWeb/Program.cs` | Crear |
| `TransmisionesWeb/appsettings.json` | Crear |
| `TransmisionesWeb/Components/_Imports.razor` | Crear |
| `TransmisionesWeb/Services/SessionStateService.cs` | Crear |
| `TransmisionesWeb/Models/ApiModels.cs` | Crear |
| `TransmisionesWeb/Services/ApiIntegracionClient.cs` | Crear |
| `TransmisionesWeb/Components/Layout/MainLayout.razor` | Crear |
| `TransmisionesWeb/Components/Layout/EmpleadoLayout.razor` | Crear |
| `TransmisionesWeb/Components/Shared/EstadoConexion.razor` | Crear |
| `TransmisionesWeb/Components/Shared/ClienteAuthGuard.razor` | Crear |
| `TransmisionesWeb/Components/Shared/EmpleadoAuthGuard.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Home.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Login.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Registro.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Cliente/Ordenes.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Cliente/Facturas.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Cliente/FacturaDetalle.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Empleado/AperturaCaja.razor` | Crear (basado en existente) |
| `TransmisionesWeb/Components/Pages/Empleado/TerminalCaja.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Empleado/FacturaEmpleado.razor` | Crear |
| `TransmisionesWeb/Components/Pages/Empleado/CierreCaja.razor` | Crear |

---

## Task 1: Backend — GET /api/Facturas/{id} en TransmisionesAPI

**Files:**
- Modify: `TransmisionesAPI/Controllers/FacturasController.cs`

- [ ] **Step 1: Agregar endpoint GET /{id} a FacturasController**

Reemplazar el contenido completo de `TransmisionesAPI/Controllers/FacturasController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using TransmisionesCore.UseCases;
using TransmisionesInfraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace TransmisionesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaUseCases _useCases;
    private readonly TransmisionesContext _context;

    public FacturasController(FacturaUseCases useCases, TransmisionesContext context)
    {
        _useCases = useCases;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Generar(int idOrden, int idEmpleado, int idCaja, string tipoFactura = "Consumidor_Final")
        => Ok(await _useCases.GenerarFacturaAsync(idOrden, idEmpleado, idCaja, tipoFactura));

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerFactura(int id)
    {
        var factura = await _context.Facturas
            .Include(f => f.Orden)
                .ThenInclude(o => o.DetallesOrden)
                    .ThenInclude(d => d.Producto)
            .Include(f => f.Orden)
                .ThenInclude(o => o.DetallesServicio)
                    .ThenInclude(ds => ds.Servicio)
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id_factura == id);

        if (factura == null)
            return NotFound(new { mensaje = $"Factura {id} no encontrada." });

        var resultado = new
        {
            factura.Id_factura,
            factura.Numero_factura,
            factura.Fecha_emision,
            factura.Subtotal,
            factura.ITBIS,
            factura.Total,
            factura.Tipo_factura,
            factura.Id_cliente,
            NombreCliente = factura.Cliente != null
                ? $"{factura.Cliente.Nombre_cliente} {factura.Cliente.Apellido_cliente}"
                : "—",
            CorreoCliente = factura.Cliente?.Correo_cliente,
            Detalles = factura.Orden?.DetallesOrden?.Select(d => new
            {
                Descripcion = d.Producto?.Nombre_producto ?? "Producto",
                d.Cantidad,
                PrecioUnitario = d.Precio_unitario,
                Subtotal = d.Cantidad * d.Precio_unitario
            }).Cast<object>()
            .Concat(factura.Orden?.DetallesServicio?.Select(ds => new
            {
                Descripcion = ds.Servicio?.Nombre_servicio ?? "Servicio",
                Cantidad = 1,
                PrecioUnitario = ds.Precio_unitario,
                Subtotal = ds.Precio_unitario
            }).Cast<object>() ?? Enumerable.Empty<object>())
            .ToList()
        };

        return Ok(resultado);
    }
}
```

- [ ] **Step 2: Verificar que compila**

```bash
cd /c/PROYECTO/TransmisionesAPI
dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`. Si falla por `Correo_cliente`, verifica el nombre del campo en `TransmisionesCore/Entities/Cliente.cs` y ajusta.

- [ ] **Step 3: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesAPI/Controllers/FacturasController.cs
git commit -m "feat(api): agregar GET /api/Facturas/{id} con detalles de orden y cliente"
```

---

## Task 2: Backend — GET /api/integracion/estado + POST gasto

**Files:**
- Create: `TransmisionesIntegracion/Controllers/IntegracionEstadoController.cs`
- Modify: `TransmisionesIntegracion/Controllers/IntegracionCajasController.cs`

- [ ] **Step 1: Crear IntegracionEstadoController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using TransmisionesIntegracion.Data;

namespace TransmisionesIntegracion.Controllers;

[ApiController]
[Route("api/integracion/estado")]
public class IntegracionEstadoController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IntegracionDbContext _db;

    public IntegracionEstadoController(IHttpClientFactory httpFactory, IntegracionDbContext db)
    {
        _httpFactory = httpFactory;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstado()
    {
        bool azureDisponible = false;
        try
        {
            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(3);
            var resp = await http.GetAsync("https://localhost:56678/api/Cajas/resumen-hoy");
            azureDisponible = resp.IsSuccessStatusCode;
        }
        catch { /* Azure no alcanzable */ }

        int pendientes = _db.TransaccionesPendientes.Count(t => !t.Sincronizado);

        return Ok(new
        {
            azureDisponible,
            transaccionesPendientes = pendientes,
            timestamp = DateTime.UtcNow
        });
    }
}
```

- [ ] **Step 2: Agregar POST gasto a IntegracionCajasController**

Dentro de la clase `IntegracionCajasController` (antes del cierre `}`), agregar:

```csharp
[HttpPost("{idCaja}/gasto")]
public async Task<IActionResult> RegistrarGasto(int idCaja, [FromBody] GastoIntegracionDto peticion)
{
    // Guardamos en cola local (siempre — Azure no tiene endpoint de gasto aún)
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
```

Al final del archivo (fuera del namespace de la clase, junto a los otros DTOs), agregar:

```csharp
public class GastoIntegracionDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}
```

- [ ] **Step 3: Compilar TransmisionesIntegracion**

```bash
cd /c/PROYECTO/TransmisionesIntegracion
dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesIntegracion/Controllers/IntegracionEstadoController.cs
git add TransmisionesIntegracion/Controllers/IntegracionCajasController.cs
git commit -m "feat(integracion): agregar endpoint estado y gasto de caja"
```

---

## Task 3: Backend — MailKit + EmailService + IntegracionFacturasController

**Files:**
- Modify: `TransmisionesIntegracion/TransmisionesIntegracion.csproj`
- Modify: `TransmisionesIntegracion/appsettings.Development.json`
- Create: `TransmisionesIntegracion/Services/EmailService.cs`
- Create: `TransmisionesIntegracion/Controllers/IntegracionFacturasController.cs`
- Modify: `TransmisionesIntegracion/Program.cs`

- [ ] **Step 1: Agregar MailKit al .csproj**

```bash
cd /c/PROYECTO/TransmisionesIntegracion
dotnet add package MailKit --version 4.8.0
```

- [ ] **Step 2: Agregar configuración email a appsettings.Development.json**

El archivo ya existe. Agregar la sección `"Email"` al JSON (dentro del objeto raíz):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Usuario": "tu-correo@gmail.com",
    "Password": "tu-app-password",
    "NombreRemitente": "Transmisiones MAG"
  }
}
```

- [ ] **Step 3: Crear EmailService.cs**

```csharp
using MailKit.Net.Smtp;
using MimeKit;

namespace TransmisionesIntegracion.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task EnviarFacturaAsync(string destinatario, string nombreCliente, object factura)
    {
        var html = GenerarHtmlFactura(nombreCliente, factura);

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(
            _config["Email:NombreRemitente"] ?? "Transmisiones MAG",
            _config["Email:Usuario"]));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = "Tu factura de Transmisiones MAG";

        var body = new BodyBuilder { HtmlBody = html };
        mensaje.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Email:Host"],
            int.Parse(_config["Email:Port"] ?? "587"),
            MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config["Email:Usuario"], _config["Email:Password"]);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);
    }

    private string GenerarHtmlFactura(string nombreCliente, dynamic f)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><style>
          body {{ font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; }}
          .header {{ background: #212529; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
          .body {{ padding: 20px; border: 1px solid #dee2e6; }}
          table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
          th {{ background: #f8f9fa; padding: 8px; text-align: left; border-bottom: 2px solid #dee2e6; }}
          td {{ padding: 8px; border-bottom: 1px solid #dee2e6; }}
          .total {{ font-size: 1.2em; font-weight: bold; color: #198754; }}
          .footer {{ text-align: center; font-size: 0.85em; color: #6c757d; margin-top: 20px; }}
        </style></head>
        <body>
          <div class="header">
            <h2>Transmisiones MAG</h2>
            <p style="margin:0">Tu comprobante de servicio</p>
          </div>
          <div class="body">
            <p>Estimado/a <strong>{nombreCliente}</strong>,</p>
            <p>Adjuntamos el detalle de tu factura <strong>#{f.Numero_factura}</strong> del {f.Fecha_emision:dd/MM/yyyy}.</p>
            <table>
              <tr><th>Descripción</th><th>Cant.</th><th>Precio</th><th>Subtotal</th></tr>
              <!-- Detalles renderizados por el controller -->
              {f.FilasHtml}
            </table>
            <hr/>
            <p>Subtotal: <strong>RD$ {f.Subtotal:N2}</strong></p>
            <p>ITBIS (18%): <strong>RD$ {f.ITBIS:N2}</strong></p>
            <p class="total">TOTAL: RD$ {f.Total:N2}</p>
          </div>
          <div class="footer">
            Transmisiones MAG · Santo Domingo, RD · Gracias por su preferencia
          </div>
        </body>
        </html>
        """;
    }
}
```

- [ ] **Step 4: Crear IntegracionFacturasController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text;
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

            // 1. Obtener factura
            var respFactura = await http.GetAsync($"https://localhost:56678/api/Facturas/{id}");
            if (!respFactura.IsSuccessStatusCode)
                return StatusCode(503, new { mensaje = "No se pudo obtener la factura. Verifica conexión." });

            var jsonFactura = await respFactura.Content.ReadAsStringAsync();
            using var docFactura = JsonDocument.Parse(jsonFactura);
            var root = docFactura.RootElement;

            var correo = root.GetProperty("correoCliente").GetString();
            var nombre = root.GetProperty("nombreCliente").GetString() ?? "Cliente";
            var numero = root.GetProperty("numero_factura").GetString() ?? id.ToString();
            var subtotal = root.GetProperty("subtotal").GetDecimal();
            var itbis = root.GetProperty("itbis").GetDecimal();
            var total = root.GetProperty("total").GetDecimal();
            var fecha = root.GetProperty("fecha_emision").GetDateTime();

            if (string.IsNullOrWhiteSpace(correo))
                return BadRequest(new { mensaje = "El cliente no tiene correo registrado." });

            // 2. Construir filas HTML para la tabla de detalles
            var filasHtml = new StringBuilder();
            if (root.TryGetProperty("detalles", out var detalles))
            {
                foreach (var detalle in detalles.EnumerateArray())
                {
                    var desc = detalle.GetProperty("descripcion").GetString();
                    var cant = detalle.GetProperty("cantidad").GetInt32();
                    var precio = detalle.GetProperty("precioUnitario").GetDecimal();
                    var sub = detalle.GetProperty("subtotal").GetDecimal();
                    filasHtml.Append($"<tr><td>{desc}</td><td>{cant}</td><td>RD$ {precio:N2}</td><td>RD$ {sub:N2}</td></tr>");
                }
            }

            // 3. Construir objeto anónimo para el template
            var datos = new
            {
                Numero_factura = numero,
                Fecha_emision = fecha,
                Subtotal = subtotal,
                ITBIS = itbis,
                Total = total,
                FilasHtml = filasHtml.ToString()
            };

            await _email.EnviarFacturaAsync(correo, nombre, datos);
            return Ok(new { exito = true, mensaje = $"Factura enviada a {correo}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al enviar el correo: {ex.Message}" });
        }
    }
}
```

- [ ] **Step 5: Registrar EmailService en Program.cs de TransmisionesIntegracion**

En `TransmisionesIntegracion/Program.cs`, agregar después de `builder.Services.AddSwaggerGen();`:

```csharp
builder.Services.AddSingleton<TransmisionesIntegracion.Services.EmailService>();
```

- [ ] **Step 6: Compilar**

```bash
cd /c/PROYECTO/TransmisionesIntegracion
dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 7: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesIntegracion/
git commit -m "feat(integracion): agregar MailKit, EmailService y IntegracionFacturasController"
```

---

## Task 4: Crear proyecto TransmisionesWeb

**Files:**
- Create: `TransmisionesWeb/TransmisionesWeb.csproj`
- Modify: `TransmisionesSolution.sln`
- Create: `TransmisionesWeb/appsettings.json`
- Create: `TransmisionesWeb/appsettings.Development.json`
- Create: `TransmisionesWeb/Components/_Imports.razor`

- [ ] **Step 1: Crear el proyecto Blazor Server**

```bash
cd /c/PROYECTO
dotnet new blazor -n TransmisionesWeb -o TransmisionesWeb --interactivity Server --empty false 2>&1
```

- [ ] **Step 2: Agregar Polly al proyecto**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0
```

- [ ] **Step 3: Agregar el proyecto a la solución**

```bash
cd /c/PROYECTO
dotnet sln TransmisionesSolution.sln add TransmisionesWeb/TransmisionesWeb.csproj
```

- [ ] **Step 4: Reemplazar appsettings.json**

Crear `TransmisionesWeb/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiIntegracion": {
    "BaseUrl": "https://localhost:7001",
    "CajaIdDefault": 1
  }
}
```

- [ ] **Step 5: Crear appsettings.Development.json**

```json
{
  "ApiIntegracion": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

> **Nota:** El puerto `7001` debe coincidir con el de `TransmisionesIntegracion`. Verificarlo en `TransmisionesIntegracion/Properties/launchSettings.json` y actualizar aquí si es diferente.

- [ ] **Step 6: Reemplazar _Imports.razor**

`TransmisionesWeb/Components/_Imports.razor`:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using TransmisionesWeb
@using TransmisionesWeb.Components
@using TransmisionesWeb.Components.Layout
@using TransmisionesWeb.Components.Shared
@using TransmisionesWeb.Services
@using TransmisionesWeb.Models
```

- [ ] **Step 7: Verificar que el proyecto compila y corre**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 8: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/ TransmisionesSolution.sln
git commit -m "feat(web): crear proyecto TransmisionesWeb Blazor Server"
```

---

## Task 5: Services — SessionStateService, ApiModels, ApiIntegracionClient

**Files:**
- Create: `TransmisionesWeb/Services/SessionStateService.cs`
- Create: `TransmisionesWeb/Models/ApiModels.cs`
- Create: `TransmisionesWeb/Services/ApiIntegracionClient.cs`

- [ ] **Step 1: Crear SessionStateService.cs**

```csharp
namespace TransmisionesWeb.Services;

public class SessionStateService
{
    // Portal cliente
    public int?    ClienteId      { get; set; }
    public string? ClienteNombre  { get; set; }
    public string? ClienteEmail   { get; set; }

    // Terminal de caja
    public int?    EmpleadoId     { get; set; }
    public string? EmpleadoNombre { get; set; }
    public string? EmpleadoRol    { get; set; }
    public int?    CajaId         { get; set; }

    public bool EsClienteAutenticado  => ClienteId.HasValue;
    public bool EsEmpleadoAutenticado => EmpleadoId.HasValue;

    public void CerrarSesionCliente()
    {
        ClienteId     = null;
        ClienteNombre = null;
        ClienteEmail  = null;
    }

    public void CerrarSesionEmpleado()
    {
        EmpleadoId     = null;
        EmpleadoNombre = null;
        EmpleadoRol    = null;
        CajaId         = null;
    }
}
```

- [ ] **Step 2: Crear ApiModels.cs**

```csharp
namespace TransmisionesWeb.Models;

public record EstadoSistemaDto(bool AzureDisponible, int TransaccionesPendientes, DateTime Timestamp);

public record ClienteDto(
    int Id,
    string Nombre,
    string Apellido,
    string Documento,
    string? Telefono,
    string? Correo
);

public record OrdenDto(
    int Id_orden,
    string Estado,
    DateTime Fecha_ingreso,
    string? Matricula_vehiculo,
    string? Modelo_vehiculo,
    int Id_cliente
);

public record DetalleFacturaDto(
    string Descripcion,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record FacturaDto(
    int Id_factura,
    string Numero_factura,
    DateTime Fecha_emision,
    decimal Subtotal,
    decimal ITBIS,
    decimal Total,
    string Tipo_factura,
    int Id_cliente,
    string NombreCliente,
    string? CorreoCliente,
    List<DetalleFacturaDto> Detalles
);

public record LoginResponseDto(
    int IdEmpleado,
    string? Nombre,
    string? Rol,
    int? IdSucursal,
    string? NombreSucursal,
    bool ModoOffline
);

public record EstadoCajaDto(
    string CodigoCaja,
    string Estado,
    decimal SaldoInicial,
    decimal SaldoFinal
);

public record ResumenCajaDiarioDto(
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal SaldoNeto,
    int CantidadOperaciones,
    DateTime Fecha
);

public record ProductoDto(
    int Id_producto,
    string Nombre_producto,
    decimal Precio_venta,
    int Stock_actual,
    string? Categoria
);

public record ServicioDto(
    int Id_servicio,
    string Nombre_servicio,
    decimal Precio_base
);

public record ProcesarOrdenResponseDto(
    int? IdFactura,
    int? IdOrden,
    bool Exito,
    string? Mensaje
);

// Para el carrito de la caja
public class ItemCotizacion
{
    public int Id         { get; set; }
    public string Nombre  { get; set; } = string.Empty;
    public string Tipo    { get; set; } = string.Empty; // "Producto" | "Servicio"
    public int Cantidad   { get; set; } = 1;
    public decimal Precio { get; set; }
    public decimal Subtotal => Cantidad * Precio;
}
```

- [ ] **Step 3: Crear ApiIntegracionClient.cs**

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using TransmisionesWeb.Models;

namespace TransmisionesWeb.Services;

public class ApiIntegracionClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

    public ApiIntegracionClient(HttpClient http) => _http = http;

    // ─── Estado ───────────────────────────────────────────
    public async Task<EstadoSistemaDto?> GetEstadoAsync()
    {
        try { return await _http.GetFromJsonAsync<EstadoSistemaDto>("api/integracion/estado", _opts); }
        catch { return null; }
    }

    // ─── Clientes ─────────────────────────────────────────
    public async Task<ClienteDto?> BuscarClientePorDocumentoAsync(string documento)
    {
        try
        {
            var resp = await _http.GetAsync($"api/integracion/clientes/buscar/{documento}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            // El endpoint puede devolver {datos: {...}} en modo offline o el objeto directo
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

    // ─── Órdenes ──────────────────────────────────────────
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

    // ─── Facturas ─────────────────────────────────────────
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

    // ─── Autenticación ────────────────────────────────────
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

    // ─── Cajas ────────────────────────────────────────────
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

    // ─── Productos y Servicios ────────────────────────────
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

    // ─── Procesar Orden ───────────────────────────────────
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
```

- [ ] **Step 4: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/Services/ TransmisionesWeb/Models/
git commit -m "feat(web): agregar SessionStateService, ApiModels y ApiIntegracionClient"
```

---

## Task 6: Program.cs — DI y configuración

**Files:**
- Modify: `TransmisionesWeb/Program.cs`

- [ ] **Step 1: Reemplazar Program.cs con configuración completa**

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http.Polly;
using Polly;
using TransmisionesWeb.Components;
using TransmisionesWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Sesión (scoped = una por circuito SignalR)
builder.Services.AddScoped<SessionStateService>();

// HttpClient tipado con Polly (3 reintentos, backoff exponencial)
var apiUrl = builder.Configuration["ApiIntegracion:BaseUrl"]
    ?? throw new InvalidOperationException("Falta ApiIntegracion:BaseUrl en appsettings.json");

builder.Services.AddHttpClient<ApiIntegracionClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
})
.AddTransientHttpErrorPolicy(p =>
    p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1))));

// JS Interop para window.print()
builder.Services.AddScoped<IJSRuntime>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

> **Nota:** Si `dotnet build` reporta error en `AddTransientHttpErrorPolicy`, instalar: `dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0`

- [ ] **Step 2: Compilar**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/Program.cs
git commit -m "feat(web): configurar DI, HttpClient con Polly y Blazor Server"
```

---

## Task 7: Layouts + Auth Guards + EstadoConexion

**Files:**
- Create: `TransmisionesWeb/Components/Layout/MainLayout.razor`
- Create: `TransmisionesWeb/Components/Layout/EmpleadoLayout.razor`
- Create: `TransmisionesWeb/Components/Shared/EstadoConexion.razor`
- Create: `TransmisionesWeb/Components/Shared/ClienteAuthGuard.razor`
- Create: `TransmisionesWeb/Components/Shared/EmpleadoAuthGuard.razor`

- [ ] **Step 1: Crear MainLayout.razor**

```razor
@inherits LayoutComponentBase
@inject SessionStateService Session
@inject NavigationManager Nav

<nav class="navbar navbar-dark bg-dark px-4">
    <a class="navbar-brand fw-bold" href="/">
        <i class="bi bi-gear-fill text-warning me-2"></i>Transmisiones MAG
    </a>
    <div class="d-flex align-items-center gap-3">
        @if (Session.EsClienteAutenticado)
        {
            <span class="text-white-50 small">Hola, <strong class="text-white">@Session.ClienteNombre</strong></span>
            <button class="btn btn-outline-light btn-sm" @onclick="CerrarSesion">Salir</button>
        }
        else
        {
            <a class="btn btn-warning btn-sm fw-bold" href="/login">Acceder</a>
        }
    </div>
</nav>

<main class="container py-4">
    @Body
</main>

@code {
    private void CerrarSesion()
    {
        Session.CerrarSesionCliente();
        Nav.NavigateTo("/", forceLoad: true);
    }
}
```

- [ ] **Step 2: Crear EmpleadoLayout.razor**

```razor
@inherits LayoutComponentBase
@inject SessionStateService Session
@inject NavigationManager Nav

<nav class="navbar navbar-dark bg-dark px-4">
    <span class="navbar-brand fw-bold">
        <i class="bi bi-cash-register text-warning me-2"></i>Terminal de Caja
    </span>
    <div class="d-flex align-items-center gap-3">
        <EstadoConexion />
        @if (Session.EsEmpleadoAutenticado)
        {
            <span class="text-white-50 small"><strong class="text-white">@Session.EmpleadoNombre</strong></span>
            <a href="/empleado/cierre" class="btn btn-outline-warning btn-sm">Cerrar Caja</a>
        }
    </div>
</nav>

<main class="container-fluid py-3">
    @Body
</main>
```

- [ ] **Step 3: Crear EstadoConexion.razor**

```razor
@inject ApiIntegracionClient Api
@implements IDisposable

@if (_estado is not null)
{
    @if (_estado.AzureDisponible)
    {
        <span class="badge bg-success rounded-pill px-3">
            <i class="bi bi-wifi me-1"></i>En línea
        </span>
    }
    else
    {
        <span class="badge bg-danger rounded-pill px-3" title="@_estado.TransaccionesPendientes transacciones pendientes">
            <i class="bi bi-wifi-off me-1"></i>Offline — @_estado.TransaccionesPendientes pendiente(s)
        </span>
    }
}

@code {
    private EstadoSistemaDto? _estado;
    private Timer? _timer;

    protected override async Task OnInitializedAsync()
    {
        await RefrescarEstado();
        _timer = new Timer(async _ =>
        {
            await RefrescarEstado();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task RefrescarEstado() => _estado = await Api.GetEstadoAsync();

    public void Dispose() => _timer?.Dispose();
}
```

- [ ] **Step 4: Crear ClienteAuthGuard.razor**

```razor
@inject SessionStateService Session
@inject NavigationManager Nav

@if (Session.EsClienteAutenticado)
{
    @ChildContent
}

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (!Session.EsClienteAutenticado)
            Nav.NavigateTo("/login");
    }
}
```

- [ ] **Step 5: Crear EmpleadoAuthGuard.razor**

```razor
@inject SessionStateService Session
@inject NavigationManager Nav

@if (Session.EsEmpleadoAutenticado)
{
    @ChildContent
}

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        if (!Session.EsEmpleadoAutenticado)
            Nav.NavigateTo("/empleado/apertura");
    }
}
```

- [ ] **Step 6: Compilar**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet build 2>&1
```

- [ ] **Step 7: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/
git commit -m "feat(web): agregar layouts, auth guards y componente EstadoConexion"
```

---

## Task 8: Portal — Home + Login

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Home.razor`
- Create: `TransmisionesWeb/Components/Pages/Login.razor`

- [ ] **Step 1: Crear Home.razor**

```razor
@page "/"
@layout MainLayout

<div class="text-center py-5">
    <i class="bi bi-gear-fill display-1 text-warning"></i>
    <h1 class="fw-bold mt-3">Transmisiones MAG</h1>
    <p class="lead text-muted">Tu taller de confianza para transmisiones vehiculares</p>
    <a href="/login" class="btn btn-dark btn-lg px-5 mt-3 rounded-pill">
        Ver mis órdenes <i class="bi bi-arrow-right ms-2"></i>
    </a>
</div>

<div class="row g-4 mt-5">
    <div class="col-md-4">
        <div class="card border-0 shadow-sm h-100 text-center p-4">
            <i class="bi bi-tools display-4 text-warning mb-3"></i>
            <h5 class="fw-bold">Reparaciones</h5>
            <p class="text-muted small">Diagnóstico y reparación de transmisiones automáticas y manuales.</p>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card border-0 shadow-sm h-100 text-center p-4">
            <i class="bi bi-clipboard-check display-4 text-warning mb-3"></i>
            <h5 class="fw-bold">Seguimiento</h5>
            <p class="text-muted small">Consulta el estado de tu vehículo en tiempo real.</p>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card border-0 shadow-sm h-100 text-center p-4">
            <i class="bi bi-receipt display-4 text-warning mb-3"></i>
            <h5 class="fw-bold">Facturas digitales</h5>
            <p class="text-muted small">Recibe tus facturas directamente en tu correo electrónico.</p>
        </div>
    </div>
</div>
```

- [ ] **Step 2: Crear Login.razor**

```razor
@page "/login"
@layout MainLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject NavigationManager Nav

<div class="container-fluid bg-light vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow-lg border-0 rounded-4" style="max-width: 440px; width: 100%;">
        <div class="card-header bg-dark text-white text-center py-4 rounded-top-4">
            <i class="bi bi-person-badge display-4 text-warning mb-2"></i>
            <h2 class="fw-bold mb-0">Portal del Cliente</h2>
            <p class="text-white-50 small mb-0">Transmisiones MAG</p>
        </div>
        <div class="card-body p-5">
            <EditForm Model="_model" OnValidSubmit="Acceder">
                <DataAnnotationsValidator />

                <div class="mb-4">
                    <label class="form-label fw-bold small text-muted">NÚMERO DE CÉDULA</label>
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0 text-muted">
                            <i class="bi bi-card-text"></i>
                        </span>
                        <InputText @bind-Value="_model.Cedula"
                                   class="form-control border-start-0 ps-0"
                                   placeholder="000-0000000-0" />
                    </div>
                </div>

                @if (!string.IsNullOrEmpty(_error))
                {
                    <div class="alert alert-danger py-2 small">
                        <i class="bi bi-exclamation-triangle me-2"></i>@_error
                    </div>
                }

                <div class="d-grid gap-2 mt-4">
                    <button type="submit" class="btn btn-dark btn-lg py-3 fw-bold rounded-pill shadow"
                            disabled="@_cargando">
                        @if (_cargando)
                        {
                            <span class="spinner-border spinner-border-sm me-2"></span>
                        }
                        ACCEDER <i class="bi bi-arrow-right ms-2"></i>
                    </button>
                    <a href="/registro" class="btn btn-link text-muted text-decoration-none small">
                        ¿Primera vez? Regístrate aquí
                    </a>
                </div>
            </EditForm>
        </div>
    </div>
</div>

@code {
    private readonly LoginModel _model = new();
    private string _error = string.Empty;
    private bool _cargando = false;

    private async Task Acceder()
    {
        _cargando = true;
        _error = string.Empty;

        var cliente = await Api.BuscarClientePorDocumentoAsync(_model.Cedula);

        if (cliente is null)
        {
            _error = "No encontramos una cuenta con esa cédula. ¿Deseas registrarte?";
        }
        else
        {
            Session.ClienteId     = cliente.Id;
            Session.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
            Session.ClienteEmail  = cliente.Correo;
            Nav.NavigateTo("/cliente/ordenes");
        }

        _cargando = false;
    }

    private class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Ingresa tu número de cédula.")]
        public string Cedula { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 3: Compilar**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet build 2>&1
```

- [ ] **Step 4: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Home.razor TransmisionesWeb/Components/Pages/Login.razor
git commit -m "feat(web): agregar páginas Home y Login del portal cliente"
```

---

## Task 9: Portal — Registro

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Registro.razor`

- [ ] **Step 1: Crear Registro.razor**

```razor
@page "/registro"
@layout MainLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject NavigationManager Nav

<div class="container py-4" style="max-width: 600px;">
    <div class="card shadow-lg border-0 rounded-4">
        <div class="card-header bg-dark text-white text-center py-4 rounded-top-4">
            <i class="bi bi-person-plus-fill display-4 text-warning mb-2"></i>
            <h2 class="fw-bold mb-0">Crear Cuenta</h2>
        </div>
        <div class="card-body p-4">
            <EditForm Model="_model" OnValidSubmit="Registrar">
                <DataAnnotationsValidator />
                <ValidationSummary class="alert alert-warning small" />

                <div class="row g-3">
                    <div class="col-md-6">
                        <label class="form-label fw-bold small text-muted">NOMBRE</label>
                        <InputText @bind-Value="_model.NombreCliente" class="form-control rounded-3" placeholder="Juan" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label fw-bold small text-muted">APELLIDO</label>
                        <InputText @bind-Value="_model.ApellidoCliente" class="form-control rounded-3" placeholder="Pérez" />
                    </div>
                    <div class="col-12">
                        <label class="form-label fw-bold small text-muted">CÉDULA</label>
                        <InputText @bind-Value="_model.Cedula" class="form-control rounded-3" placeholder="000-0000000-0" />
                    </div>
                    <div class="col-12">
                        <label class="form-label fw-bold small text-muted">TELÉFONO</label>
                        <InputText @bind-Value="_model.Telefono" class="form-control rounded-3" placeholder="809-000-0000" />
                    </div>
                    <div class="col-12">
                        <label class="form-label fw-bold small text-muted">CORREO ELECTRÓNICO</label>
                        <InputText @bind-Value="_model.Correo" class="form-control rounded-3" placeholder="juan@ejemplo.com" />
                    </div>
                </div>

                @if (!string.IsNullOrEmpty(_mensaje))
                {
                    <div class="alert @(_exito ? "alert-success" : "alert-danger") mt-3 small">
                        @_mensaje
                    </div>
                }

                <div class="d-grid mt-4">
                    <button type="submit" class="btn btn-dark btn-lg rounded-pill fw-bold"
                            disabled="@_cargando">
                        @if (_cargando) { <span class="spinner-border spinner-border-sm me-2"></span> }
                        CREAR CUENTA
                    </button>
                    <a href="/login" class="btn btn-link text-muted text-decoration-none small mt-2">
                        Ya tengo cuenta
                    </a>
                </div>
            </EditForm>
        </div>
    </div>
</div>

@code {
    private readonly RegistroModel _model = new();
    private string _mensaje = string.Empty;
    private bool _exito = false;
    private bool _cargando = false;

    private async Task Registrar()
    {
        _cargando = true;
        _mensaje  = string.Empty;

        var payload = new
        {
            IdSector     = 1,  // Valor por defecto — ajustar con selector en mejora futura
            IdMunicipio  = 1,
            IdProvincia  = 1,
            _model.NombreCliente,
            _model.ApellidoCliente,
            Cedula       = _model.Cedula,
            _model.Telefono,
            Correo       = _model.Correo
        };

        bool ok = await Api.RegistrarClienteAsync(payload);

        if (ok)
        {
            _exito   = true;
            _mensaje = "Cuenta creada exitosamente. Redirigiendo...";
            // Hacer login automático
            var cliente = await Api.BuscarClientePorDocumentoAsync(_model.Cedula);
            if (cliente is not null)
            {
                Session.ClienteId     = cliente.Id;
                Session.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                Session.ClienteEmail  = cliente.Correo;
                await Task.Delay(1000);
                Nav.NavigateTo("/cliente/ordenes");
            }
        }
        else
        {
            _exito   = false;
            _mensaje = "Ocurrió un error al crear la cuenta. Inténtalo de nuevo.";
        }

        _cargando = false;
    }

    private class RegistroModel
    {
        [System.ComponentModel.DataAnnotations.Required] public string NombreCliente  { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string ApellidoCliente { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Cedula         { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        [System.ComponentModel.DataAnnotations.EmailAddress] public string? Correo    { get; set; }
    }
}
```

- [ ] **Step 2: Compilar y commit**

```bash
cd /c/PROYECTO/TransmisionesWeb
dotnet build 2>&1
```
```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Registro.razor
git commit -m "feat(web): agregar página de registro de cliente"
```

---

## Task 10: Portal — Ordenes + Facturas (lista)

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Cliente/Ordenes.razor`
- Create: `TransmisionesWeb/Components/Pages/Cliente/Facturas.razor`

- [ ] **Step 1: Crear directorio y Ordenes.razor**

```bash
mkdir -p /c/PROYECTO/TransmisionesWeb/Components/Pages/Cliente
```

```razor
@page "/cliente/ordenes"
@layout MainLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session

<ClienteAuthGuard>
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h3 class="fw-bold mb-0"><i class="bi bi-clipboard-list me-2 text-warning"></i>Mis Órdenes</h3>
        <a href="/cliente/facturas" class="btn btn-outline-dark btn-sm">
            <i class="bi bi-receipt me-1"></i>Ver Facturas
        </a>
    </div>

    @if (_cargando)
    {
        <div class="text-center py-5">
            <div class="spinner-border text-warning" role="status"></div>
            <p class="mt-2 text-muted">Cargando órdenes...</p>
        </div>
    }
    else if (!_ordenes.Any())
    {
        <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>No tienes órdenes registradas aún.
        </div>
    }
    else
    {
        <div class="row g-3">
            @foreach (var orden in _ordenes)
            {
                <div class="col-12">
                    <div class="card border-0 shadow-sm rounded-4 p-3">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <h6 class="fw-bold mb-1">
                                    <i class="bi bi-car-front me-2 text-muted"></i>
                                    @(orden.Matricula_vehiculo ?? "Vehículo") — @(orden.Modelo_vehiculo ?? "")
                                </h6>
                                <small class="text-muted">
                                    Ingresado: @orden.Fecha_ingreso.ToString("dd/MM/yyyy")
                                </small>
                            </div>
                            <div class="d-flex flex-column align-items-end gap-2">
                                <span class="badge rounded-pill @BadgeClass(orden.Estado) px-3">
                                    @orden.Estado
                                </span>
                                @if (orden.Estado == "Entregada")
                                {
                                    <a href="/cliente/factura/@orden.Id_orden" class="btn btn-sm btn-outline-success rounded-pill">
                                        <i class="bi bi-receipt me-1"></i>Ver Factura
                                    </a>
                                }
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
</ClienteAuthGuard>

@code {
    private List<OrdenDto> _ordenes = new();
    private bool _cargando = true;

    protected override async Task OnInitializedAsync()
    {
        if (Session.ClienteId.HasValue)
            _ordenes = await Api.GetOrdenesPorClienteAsync(Session.ClienteId.Value);
        _cargando = false;
    }

    private static string BadgeClass(string estado) => estado switch
    {
        "En proceso"  => "bg-warning text-dark",
        "Lista"       => "bg-info text-dark",
        "Entregada"   => "bg-success",
        "Cancelada"   => "bg-secondary",
        _             => "bg-secondary"
    };
}
```

- [ ] **Step 2: Crear Facturas.razor**

```razor
@page "/cliente/facturas"
@layout MainLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session

<ClienteAuthGuard>
    <h3 class="fw-bold mb-4"><i class="bi bi-receipt me-2 text-warning"></i>Mis Facturas</h3>

    @if (_cargando)
    {
        <div class="text-center py-5">
            <div class="spinner-border text-warning"></div>
        </div>
    }
    else
    {
        @* Filtramos órdenes entregadas = tienen factura *@
        var ordenesFacturadas = _ordenes.Where(o => o.Estado == "Entregada").ToList();

        @if (!ordenesFacturadas.Any())
        {
            <div class="alert alert-info">
                <i class="bi bi-info-circle me-2"></i>Aún no tienes facturas emitidas.
            </div>
        }
        else
        {
            <div class="list-group shadow-sm">
                @foreach (var orden in ordenesFacturadas)
                {
                    <a href="/cliente/factura/@orden.Id_orden"
                       class="list-group-item list-group-item-action d-flex justify-content-between align-items-center rounded-3 mb-2">
                        <div>
                            <i class="bi bi-file-earmark-text me-2 text-success"></i>
                            <strong>Orden #@orden.Id_orden</strong>
                            <span class="text-muted ms-2 small">— @orden.Fecha_ingreso.ToString("dd/MM/yyyy")</span>
                        </div>
                        <i class="bi bi-chevron-right text-muted"></i>
                    </a>
                }
            </div>
        }
    }
</ClienteAuthGuard>

@code {
    private List<OrdenDto> _ordenes = new();
    private bool _cargando = true;

    protected override async Task OnInitializedAsync()
    {
        if (Session.ClienteId.HasValue)
            _ordenes = await Api.GetOrdenesPorClienteAsync(Session.ClienteId.Value);
        _cargando = false;
    }
}
```

- [ ] **Step 3: Compilar y commit**

```bash
cd /c/PROYECTO/TransmisionesWeb && dotnet build 2>&1
```
```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Cliente/
git commit -m "feat(web): agregar páginas Ordenes y Facturas del portal cliente"
```

---

## Task 11: Portal — FacturaDetalle (imprimible + email)

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Cliente/FacturaDetalle.razor`

- [ ] **Step 1: Crear FacturaDetalle.razor**

```razor
@page "/cliente/factura/{IdOrden:int}"
@layout MainLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject IJSRuntime JS

<ClienteAuthGuard>
    @if (_cargando)
    {
        <div class="text-center py-5"><div class="spinner-border text-warning"></div></div>
    }
    else if (_factura is null)
    {
        <div class="alert alert-warning">
            <i class="bi bi-wifi-off me-2"></i>
            Factura no disponible en este momento. Se requiere conexión a internet.
            <button class="btn btn-sm btn-warning ms-3" @onclick="CargarFactura">Reintentar</button>
        </div>
    }
    else
    {
        <div class="d-print-none d-flex gap-2 mb-3">
            <button class="btn btn-dark rounded-pill" @onclick="Imprimir">
                <i class="bi bi-printer me-2"></i>Imprimir
            </button>
            <button class="btn btn-outline-success rounded-pill" @onclick="EnviarEmail"
                    disabled="@_enviandoEmail">
                @if (_enviandoEmail)
                { <span class="spinner-border spinner-border-sm me-1"></span> }
                <i class="bi bi-envelope me-2"></i>Enviar al correo
            </button>
        </div>

        @if (!string.IsNullOrEmpty(_mensajeEmail))
        {
            <div class="alert @(_emailExito ? "alert-success" : "alert-danger") d-print-none small">
                @_mensajeEmail
            </div>
        }

        <!-- Factura imprimible -->
        <div class="card border-0 shadow rounded-4" id="factura-print">
            <div class="card-header bg-dark text-white text-center py-4">
                <h3 class="fw-bold mb-0">Transmisiones MAG</h3>
                <p class="mb-0 text-white-50 small">Santo Domingo, República Dominicana</p>
            </div>
            <div class="card-body p-4">
                <div class="row mb-4">
                    <div class="col-6">
                        <small class="text-muted fw-bold">FACTURA</small>
                        <p class="mb-0 fw-bold fs-5">#@_factura.Numero_factura</p>
                    </div>
                    <div class="col-6 text-end">
                        <small class="text-muted fw-bold">FECHA</small>
                        <p class="mb-0">@_factura.Fecha_emision.ToString("dd/MM/yyyy")</p>
                    </div>
                </div>
                <div class="mb-4">
                    <small class="text-muted fw-bold">CLIENTE</small>
                    <p class="mb-0 fw-bold">@_factura.NombreCliente</p>
                    @if (!string.IsNullOrEmpty(_factura.CorreoCliente))
                    {
                        <p class="mb-0 small text-muted">@_factura.CorreoCliente</p>
                    }
                </div>
                <table class="table table-sm">
                    <thead class="table-dark">
                        <tr>
                            <th>Descripción</th>
                            <th class="text-end">Cant.</th>
                            <th class="text-end">Precio</th>
                            <th class="text-end">Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var d in _factura.Detalles)
                        {
                            <tr>
                                <td>@d.Descripcion</td>
                                <td class="text-end">@d.Cantidad</td>
                                <td class="text-end">RD$ @d.PrecioUnitario.ToString("N2")</td>
                                <td class="text-end">RD$ @d.Subtotal.ToString("N2")</td>
                            </tr>
                        }
                    </tbody>
                    <tfoot>
                        <tr><td colspan="3" class="text-end fw-bold">Subtotal</td><td class="text-end">RD$ @_factura.Subtotal.ToString("N2")</td></tr>
                        <tr><td colspan="3" class="text-end fw-bold">ITBIS (18%)</td><td class="text-end">RD$ @_factura.ITBIS.ToString("N2")</td></tr>
                        <tr class="table-dark fw-bold fs-5">
                            <td colspan="3" class="text-end">TOTAL</td>
                            <td class="text-end">RD$ @_factura.Total.ToString("N2")</td>
                        </tr>
                    </tfoot>
                </table>
                <p class="text-center text-muted small mt-3">Tipo: @_factura.Tipo_factura · Gracias por preferirnos</p>
            </div>
        </div>
    }
</ClienteAuthGuard>

@code {
    [Parameter] public int IdOrden { get; set; }

    private FacturaDto? _factura;
    private bool _cargando = true;
    private bool _enviandoEmail = false;
    private string _mensajeEmail = string.Empty;
    private bool _emailExito = false;

    protected override async Task OnInitializedAsync() => await CargarFactura();

    private async Task CargarFactura()
    {
        _cargando = true;
        _factura = await Api.GetFacturaAsync(IdOrden);
        _cargando = false;
    }

    private async Task Imprimir() => await JS.InvokeVoidAsync("window.print");

    private async Task EnviarEmail()
    {
        if (_factura is null) return;
        _enviandoEmail = true;
        var (exito, msg) = await Api.EnviarFacturaPorEmailAsync(_factura.Id_factura);
        _emailExito = exito;
        _mensajeEmail = msg;
        _enviandoEmail = false;
    }
}
```

- [ ] **Step 2: Compilar y commit**

```bash
cd /c/PROYECTO/TransmisionesWeb && dotnet build 2>&1
```
```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Cliente/FacturaDetalle.razor
git commit -m "feat(web): agregar FacturaDetalle con impresión y envío de email"
```

---

## Task 12: Caja — AperturaCaja

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Empleado/AperturaCaja.razor`

- [ ] **Step 1: Crear directorio Empleado**

```bash
mkdir -p /c/PROYECTO/TransmisionesWeb/Components/Pages/Empleado
```

- [ ] **Step 2: Crear AperturaCaja.razor**

Basado en el diseño existente en `C:\Users\anpro\TransmisionesWeb\Components\Pages\Empleado\AperturaCaja.razor`, con password y llamada a la API:

```razor
@page "/empleado/apertura"
@layout EmpleadoLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject NavigationManager Nav
@inject IConfiguration Config

<div class="container-fluid bg-light vh-100 d-flex align-items-center justify-content-center">
    <div class="card shadow-lg border-0 rounded-4" style="max-width: 450px; width: 100%;">
        <div class="card-header bg-dark text-white text-center py-4 rounded-top-4">
            <i class="bi bi-unlock-fill display-4 text-warning mb-2"></i>
            <h2 class="fw-bold mb-0">Apertura de Caja</h2>
            <p class="text-white-50 small mb-0">Transmisiones MAG - Gestión Operativa</p>
        </div>
        <div class="card-body p-5">
            <EditForm Model="_model" OnValidSubmit="IniciarJornada">
                <DataAnnotationsValidator />

                <div class="mb-4">
                    <label class="form-label fw-bold small text-muted">CÓDIGO DE EMPLEADO</label>
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0 text-muted">
                            <i class="bi bi-person-badge"></i>
                        </span>
                        <InputText @bind-Value="_model.Usuario"
                                   class="form-control border-start-0 ps-0"
                                   placeholder="Ej: EMP-001" />
                    </div>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-bold small text-muted">CONTRASEÑA</label>
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0 text-muted">
                            <i class="bi bi-lock"></i>
                        </span>
                        <InputText type="password" @bind-Value="_model.Password"
                                   class="form-control border-start-0 ps-0"
                                   placeholder="••••••••" />
                    </div>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-bold small text-muted">MONTO INICIAL EN CAJA (RD$)</label>
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0 text-muted">
                            <i class="bi bi-cash-stack"></i>
                        </span>
                        <InputNumber @bind-Value="_model.MontoInicial"
                                     class="form-control border-start-0 ps-0 fs-4 fw-bold text-success"
                                     placeholder="0.00" />
                    </div>
                    <div class="form-text mt-2 small">Ingrese el fondo de sencillo disponible.</div>
                </div>

                @if (!string.IsNullOrEmpty(_error))
                {
                    <div class="alert alert-danger py-2 small">
                        <i class="bi bi-exclamation-triangle me-2"></i>@_error
                    </div>
                }

                <div class="d-grid gap-2 mt-4">
                    <button type="submit" class="btn btn-dark btn-lg py-3 fw-bold rounded-pill shadow"
                            disabled="@_cargando">
                        @if (_cargando) { <span class="spinner-border spinner-border-sm me-2"></span> }
                        INICIAR JORNADA <i class="bi bi-arrow-right ms-2"></i>
                    </button>
                    <a href="/" class="btn btn-link text-muted text-decoration-none small">
                        Volver al Portal Principal
                    </a>
                </div>
            </EditForm>
        </div>
        <div class="card-footer bg-white border-0 text-center pb-4">
            <span class="badge bg-info-subtle text-info rounded-pill px-3 py-2">
                Sucursal: Santo Domingo Centro
            </span>
        </div>
    </div>
</div>

@code {
    private readonly AperturaModel _model = new();
    private string _error = string.Empty;
    private bool _cargando = false;

    private async Task IniciarJornada()
    {
        _cargando = true;
        _error    = string.Empty;

        var login = await Api.LoginAsync(_model.Usuario, _model.Password);

        if (login is null)
        {
            _error = "Código o contraseña incorrectos. Intente de nuevo.";
            _cargando = false;
            return;
        }

        Session.EmpleadoId     = login.IdEmpleado;
        Session.EmpleadoNombre = login.Nombre ?? _model.Usuario;
        Session.EmpleadoRol    = login.Rol;

        // Usar CajaId de config (valor por defecto: 1)
        int cajaId = int.TryParse(Config["ApiIntegracion:CajaIdDefault"], out var c) ? c : 1;
        Session.CajaId = cajaId;

        bool abierta = await Api.AbrirCajaAsync(cajaId, login.IdEmpleado, _model.MontoInicial);

        if (!abierta)
        {
            _error = "No se pudo abrir la caja. Es posible que ya esté abierta o que no haya conexión.";
            Session.CerrarSesionEmpleado();
            _cargando = false;
            return;
        }

        Nav.NavigateTo("/empleado/caja");
    }

    private class AperturaModel
    {
        [System.ComponentModel.DataAnnotations.Required] public string  Usuario      { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string  Password     { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)] public decimal MontoInicial { get; set; }
    }
}

<style>
    .input-group-text { border-radius: 0.75rem 0 0 0.75rem; }
    .form-control { border-radius: 0 0.75rem 0.75rem 0; padding: 0.75rem; }
    .form-control:focus { box-shadow: none; border-color: #dee2e6; }
</style>
```

- [ ] **Step 3: Compilar y commit**

```bash
cd /c/PROYECTO/TransmisionesWeb && dotnet build 2>&1
```
```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Empleado/AperturaCaja.razor
git commit -m "feat(web): agregar AperturaCaja con login de empleado y apertura de caja"
```

---

## Task 13: Caja — TerminalCaja

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Empleado/TerminalCaja.razor`

- [ ] **Step 1: Crear TerminalCaja.razor**

```razor
@page "/empleado/caja"
@layout EmpleadoLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject NavigationManager Nav

<EmpleadoAuthGuard>
    <div class="row g-3" style="min-height: 80vh;">

        <!-- Columna izquierda: búsqueda -->
        <div class="col-md-5">
            <div class="card border-0 shadow-sm rounded-4 h-100">
                <div class="card-header bg-dark text-white rounded-top-4 py-3">
                    <h5 class="mb-0"><i class="bi bi-search me-2 text-warning"></i>Agregar ítem</h5>
                </div>
                <div class="card-body p-3">
                    <input class="form-control mb-3" placeholder="Buscar producto o servicio..."
                           @bind="_busqueda" @bind:event="oninput"
                           @oninput="BuscarAsync" />

                    @if (_buscando)
                    {
                        <div class="text-center py-3"><div class="spinner-border spinner-border-sm text-warning"></div></div>
                    }
                    else
                    {
                        @if (_productos.Any())
                        {
                            <p class="small fw-bold text-muted mb-2">PRODUCTOS</p>
                            @foreach (var p in _productos)
                            {
                                <div class="d-flex justify-content-between align-items-center p-2 mb-1 rounded-3 border hover-item"
                                     style="cursor:pointer" @onclick="() => AgregarProducto(p)">
                                    <div>
                                        <span class="fw-bold small">@p.Nombre_producto</span>
                                        <br/><span class="text-muted" style="font-size:0.75rem">Stock: @p.Stock_actual</span>
                                    </div>
                                    <span class="badge bg-dark rounded-pill">RD$ @p.Precio_venta.ToString("N2")</span>
                                </div>
                            }
                        }
                        @if (_servicios.Any())
                        {
                            <p class="small fw-bold text-muted mb-2 mt-3">SERVICIOS</p>
                            @foreach (var s in _servicios)
                            {
                                <div class="d-flex justify-content-between align-items-center p-2 mb-1 rounded-3 border hover-item"
                                     style="cursor:pointer" @onclick="() => AgregarServicio(s)">
                                    <span class="fw-bold small">@s.Nombre_servicio</span>
                                    <span class="badge bg-success rounded-pill">RD$ @s.Precio_base.ToString("N2")</span>
                                </div>
                            }
                        }
                        @if (!_productos.Any() && !_servicios.Any() && !string.IsNullOrWhiteSpace(_busqueda))
                        {
                            <p class="text-muted small text-center">Sin resultados para "@_busqueda"</p>
                        }
                    }
                </div>
            </div>
        </div>

        <!-- Columna derecha: cotización -->
        <div class="col-md-7">
            <div class="card border-0 shadow-sm rounded-4 h-100">
                <div class="card-header bg-dark text-white rounded-top-4 py-3 d-flex justify-content-between">
                    <h5 class="mb-0"><i class="bi bi-cart3 me-2 text-warning"></i>Cotización en curso</h5>
                    <button class="btn btn-sm btn-outline-light rounded-pill" @onclick="Limpiar">
                        <i class="bi bi-trash me-1"></i>Limpiar
                    </button>
                </div>
                <div class="card-body p-3">
                    @if (!_carrito.Any())
                    {
                        <div class="text-center text-muted py-5">
                            <i class="bi bi-cart-x display-4 d-block mb-2"></i>
                            Agrega productos o servicios desde la búsqueda
                        </div>
                    }
                    else
                    {
                        <div class="table-responsive">
                            <table class="table table-sm">
                                <thead><tr><th>Ítem</th><th>Cant.</th><th>Precio</th><th>Sub.</th><th></th></tr></thead>
                                <tbody>
                                    @foreach (var item in _carrito)
                                    {
                                        <tr>
                                            <td>
                                                <span class="badge @(item.Tipo == "Producto" ? "bg-dark" : "bg-success") me-1">@item.Tipo[0]</span>
                                                @item.Nombre
                                            </td>
                                            <td style="width:70px">
                                                <input type="number" min="1" class="form-control form-control-sm"
                                                       value="@item.Cantidad"
                                                       @onchange="e => item.Cantidad = int.TryParse(e.Value?.ToString(), out var v) ? v : 1" />
                                            </td>
                                            <td style="width:100px">
                                                <input type="number" step="0.01" class="form-control form-control-sm"
                                                       value="@item.Precio"
                                                       @onchange="e => item.Precio = decimal.TryParse(e.Value?.ToString(), out var v) ? v : item.Precio" />
                                            </td>
                                            <td class="text-end">RD$ @item.Subtotal.ToString("N2")</td>
                                            <td>
                                                <button class="btn btn-sm btn-outline-danger"
                                                        @onclick="() => _carrito.Remove(item)">
                                                    <i class="bi bi-x"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>

                        <div class="border-top pt-3 text-end">
                            <p class="mb-1">Subtotal: <strong>RD$ @Subtotal.ToString("N2")</strong></p>
                            <p class="mb-1">ITBIS (18%): <strong>RD$ @ITBIS.ToString("N2")</strong></p>
                            <p class="fs-4 fw-bold text-success">TOTAL: RD$ @Total.ToString("N2")</p>
                        </div>
                    }
                </div>
                <div class="card-footer bg-white border-0 p-3">
                    @if (!string.IsNullOrEmpty(_error))
                    {
                        <div class="alert alert-danger py-2 small mb-2">@_error</div>
                    }
                    <div class="d-flex gap-2">
                        <button class="btn btn-warning fw-bold rounded-pill flex-grow-1"
                                @onclick="AbrirModalGasto">
                            <i class="bi bi-dash-circle me-1"></i>Registrar Gasto
                        </button>
                        <button class="btn btn-dark fw-bold rounded-pill flex-grow-1"
                                @onclick="ConfirmarOrden" disabled="@(!_carrito.Any() || _procesando)">
                            @if (_procesando) { <span class="spinner-border spinner-border-sm me-1"></span> }
                            <i class="bi bi-check-circle me-1"></i>Confirmar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal Gasto -->
    @if (_modalGasto)
    {
        <div class="modal fade show d-block" tabindex="-1" style="background:rgba(0,0,0,0.5)">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content rounded-4 border-0 shadow-lg">
                    <div class="modal-header bg-dark text-white rounded-top-4">
                        <h5 class="modal-title"><i class="bi bi-dash-circle me-2 text-warning"></i>Registrar Gasto</h5>
                        <button class="btn-close btn-close-white" @onclick="CerrarModalGasto"></button>
                    </div>
                    <div class="modal-body p-4">
                        <div class="mb-3">
                            <label class="form-label fw-bold small text-muted">CONCEPTO</label>
                            <input class="form-control rounded-3" @bind="_gastoConcepto" placeholder="Ej: Materiales de limpieza" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small text-muted">MONTO (RD$)</label>
                            <input type="number" step="0.01" class="form-control rounded-3 fs-4 fw-bold text-danger"
                                   @bind="_gastoMonto" placeholder="0.00" />
                        </div>
                    </div>
                    <div class="modal-footer border-0">
                        <button class="btn btn-secondary rounded-pill" @onclick="CerrarModalGasto">Cancelar</button>
                        <button class="btn btn-dark rounded-pill fw-bold" @onclick="GuardarGasto"
                                disabled="@(string.IsNullOrWhiteSpace(_gastoConcepto) || _gastoMonto <= 0)">
                            Registrar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    }
</EmpleadoAuthGuard>

@code {
    private string _busqueda     = string.Empty;
    private bool   _buscando     = false;
    private bool   _procesando   = false;
    private string _error        = string.Empty;

    private List<ProductoDto>   _productos = new();
    private List<ServicioDto>   _servicios = new();
    private List<ItemCotizacion> _carrito  = new();

    private bool    _modalGasto    = false;
    private string  _gastoConcepto = string.Empty;
    private decimal _gastoMonto    = 0;

    private decimal Subtotal => _carrito.Sum(i => i.Subtotal);
    private decimal ITBIS    => Math.Round(Subtotal * 0.18m, 2);
    private decimal Total    => Subtotal + ITBIS;

    private Timer? _debounce;

    private void BuscarAsync(ChangeEventArgs e)
    {
        _busqueda = e.Value?.ToString() ?? string.Empty;
        _debounce?.Dispose();
        _debounce = new Timer(async _ =>
        {
            if (_busqueda.Length >= 2)
            {
                _buscando  = true;
                await InvokeAsync(StateHasChanged);
                _productos = await Api.BuscarProductosAsync(_busqueda);
                _servicios = await Api.GetServiciosAsync();
                _servicios = _servicios.Where(s => s.Nombre_servicio.Contains(_busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else { _productos = new(); _servicios = new(); }
            _buscando = false;
            await InvokeAsync(StateHasChanged);
        }, null, 300, Timeout.Infinite);
    }

    private void AgregarProducto(ProductoDto p) =>
        _carrito.Add(new ItemCotizacion { Id = p.Id_producto, Nombre = p.Nombre_producto, Tipo = "Producto", Precio = p.Precio_venta });

    private void AgregarServicio(ServicioDto s) =>
        _carrito.Add(new ItemCotizacion { Id = s.Id_servicio, Nombre = s.Nombre_servicio, Tipo = "Servicio", Precio = s.Precio_base });

    private void Limpiar() { _carrito.Clear(); _error = string.Empty; }

    private async Task ConfirmarOrden()
    {
        if (!Session.EmpleadoId.HasValue || !Session.CajaId.HasValue) return;
        _procesando = true;
        _error = string.Empty;

        var payload = new
        {
            IdCliente    = 1, // cliente genérico (consumidor final) — ajustar con selector en mejora futura
            IdEmpleado   = Session.EmpleadoId.Value,
            IdCanalVenta = 1,
            Matricula    = (string?)null,
            Productos = _carrito.Where(i => i.Tipo == "Producto").Select(i => new
            {
                IdProducto   = i.Id,
                i.Cantidad,
                PrecioUnitario = i.Precio
            }).ToList(),
            Servicios = _carrito.Where(i => i.Tipo == "Servicio").Select(i => new
            {
                IdServicio   = i.Id,
                IdTecnico    = Session.EmpleadoId.Value,
                PrecioUnitario = i.Precio
            }).ToList()
        };

        var result = await Api.ProcesarOrdenAsync(payload);

        if (result?.Exito == true && result.IdFactura.HasValue)
        {
            _carrito.Clear();
            Nav.NavigateTo($"/empleado/factura/{result.IdFactura.Value}");
        }
        else
        {
            _error = result?.Mensaje ?? "Error al procesar la orden. Inténtalo de nuevo.";
        }

        _procesando = false;
    }

    private void AbrirModalGasto()  { _gastoConcepto = string.Empty; _gastoMonto = 0; _modalGasto = true; }
    private void CerrarModalGasto() => _modalGasto = false;

    private async Task GuardarGasto()
    {
        if (!Session.CajaId.HasValue) return;
        await Api.RegistrarGastoAsync(Session.CajaId.Value, _gastoConcepto, _gastoMonto);
        CerrarModalGasto();
    }
}

<style>
    .hover-item:hover { background-color: #f8f9fa; transition: background 0.15s; }
</style>
```

- [ ] **Step 2: Compilar y commit**

```bash
cd /c/PROYECTO/TransmisionesWeb && dotnet build 2>&1
```
```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Empleado/TerminalCaja.razor
git commit -m "feat(web): agregar TerminalCaja con cotización, búsqueda y gastos"
```

---

## Task 14: Caja — FacturaEmpleado + CierreCaja

**Files:**
- Create: `TransmisionesWeb/Components/Pages/Empleado/FacturaEmpleado.razor`
- Create: `TransmisionesWeb/Components/Pages/Empleado/CierreCaja.razor`

- [ ] **Step 1: Crear FacturaEmpleado.razor**

```razor
@page "/empleado/factura/{IdFactura:int}"
@layout EmpleadoLayout
@inject ApiIntegracionClient Api
@inject IJSRuntime JS

<EmpleadoAuthGuard>
    @if (_cargando)
    {
        <div class="text-center py-5"><div class="spinner-border text-warning"></div></div>
    }
    else if (_factura is null)
    {
        <div class="alert alert-warning">
            No se pudo cargar la factura. <a href="/empleado/caja">Volver a caja</a>
        </div>
    }
    else
    {
        <div class="d-print-none d-flex gap-2 mb-3">
            <button class="btn btn-dark rounded-pill" @onclick="Imprimir">
                <i class="bi bi-printer me-2"></i>Imprimir
            </button>
            <button class="btn btn-outline-success rounded-pill" @onclick="EnviarEmail"
                    disabled="@_enviandoEmail">
                @if (_enviandoEmail) { <span class="spinner-border spinner-border-sm me-1"></span> }
                <i class="bi bi-envelope me-2"></i>Enviar al cliente
            </button>
            <a href="/empleado/caja" class="btn btn-outline-secondary rounded-pill ms-auto">
                <i class="bi bi-plus-circle me-1"></i>Nueva Operación
            </a>
        </div>

        @if (!string.IsNullOrEmpty(_mensajeEmail))
        {
            <div class="alert @(_emailExito ? "alert-success" : "alert-danger") small d-print-none">@_mensajeEmail</div>
        }

        <div class="card border-0 shadow rounded-4">
            <div class="card-header bg-dark text-white text-center py-4">
                <h3 class="fw-bold mb-0">Transmisiones MAG</h3>
                <p class="mb-0 text-white-50 small">Comprobante de Venta</p>
            </div>
            <div class="card-body p-4">
                <div class="row mb-4">
                    <div class="col-6">
                        <small class="text-muted fw-bold">FACTURA</small>
                        <p class="mb-0 fw-bold fs-5">#@_factura.Numero_factura</p>
                    </div>
                    <div class="col-6 text-end">
                        <small class="text-muted fw-bold">FECHA</small>
                        <p class="mb-0">@_factura.Fecha_emision.ToString("dd/MM/yyyy HH:mm")</p>
                    </div>
                </div>
                <div class="mb-3">
                    <small class="text-muted fw-bold">CLIENTE</small>
                    <p class="mb-0 fw-bold">@_factura.NombreCliente</p>
                </div>
                <table class="table table-sm">
                    <thead class="table-dark">
                        <tr><th>Descripción</th><th class="text-end">Cant.</th><th class="text-end">Precio</th><th class="text-end">Sub.</th></tr>
                    </thead>
                    <tbody>
                        @foreach (var d in _factura.Detalles)
                        {
                            <tr>
                                <td>@d.Descripcion</td>
                                <td class="text-end">@d.Cantidad</td>
                                <td class="text-end">RD$ @d.PrecioUnitario.ToString("N2")</td>
                                <td class="text-end">RD$ @d.Subtotal.ToString("N2")</td>
                            </tr>
                        }
                    </tbody>
                    <tfoot>
                        <tr><td colspan="3" class="text-end fw-bold">Subtotal</td><td class="text-end">RD$ @_factura.Subtotal.ToString("N2")</td></tr>
                        <tr><td colspan="3" class="text-end fw-bold">ITBIS 18%</td><td class="text-end">RD$ @_factura.ITBIS.ToString("N2")</td></tr>
                        <tr class="table-dark fw-bold fs-5">
                            <td colspan="3" class="text-end">TOTAL</td>
                            <td class="text-end">RD$ @_factura.Total.ToString("N2")</td>
                        </tr>
                    </tfoot>
                </table>
                <p class="text-center text-muted small mt-3">@_factura.Tipo_factura · Gracias por preferirnos</p>
            </div>
        </div>
    }
</EmpleadoAuthGuard>

@code {
    [Parameter] public int IdFactura { get; set; }
    private FacturaDto? _factura;
    private bool _cargando = true;
    private bool _enviandoEmail = false;
    private string _mensajeEmail = string.Empty;
    private bool _emailExito = false;

    protected override async Task OnInitializedAsync()
    {
        _factura  = await Api.GetFacturaAsync(IdFactura);
        _cargando = false;
    }

    private async Task Imprimir() => await JS.InvokeVoidAsync("window.print");

    private async Task EnviarEmail()
    {
        _enviandoEmail = true;
        var (exito, msg) = await Api.EnviarFacturaPorEmailAsync(IdFactura);
        _emailExito    = exito;
        _mensajeEmail  = msg;
        _enviandoEmail = false;
    }
}
```

- [ ] **Step 2: Crear CierreCaja.razor**

```razor
@page "/empleado/cierre"
@layout EmpleadoLayout
@inject ApiIntegracionClient Api
@inject SessionStateService Session
@inject NavigationManager Nav
@inject IJSRuntime JS

<EmpleadoAuthGuard>
    <div class="d-print-none d-flex justify-content-between align-items-center mb-4">
        <h3 class="fw-bold mb-0"><i class="bi bi-cash-stack me-2 text-warning"></i>Cuadre de Caja</h3>
        <div class="d-flex gap-2">
            <button class="btn btn-outline-dark rounded-pill" @onclick="Imprimir">
                <i class="bi bi-printer me-1"></i>Imprimir
            </button>
            <button class="btn btn-danger rounded-pill fw-bold" @onclick="CerrarCaja" disabled="@_cerrando">
                @if (_cerrando) { <span class="spinner-border spinner-border-sm me-1"></span> }
                <i class="bi bi-door-closed me-1"></i>Cerrar Caja
            </button>
        </div>
    </div>

    @if (_cargando)
    {
        <div class="text-center py-5"><div class="spinner-border text-warning"></div></div>
    }
    else
    {
        <div class="card border-0 shadow rounded-4" id="cuadre-print">
            <div class="card-header bg-dark text-white text-center py-4">
                <h4 class="fw-bold mb-0">Transmisiones MAG — Cuadre Diario</h4>
                <p class="mb-0 text-white-50 small">@DateTime.Now.ToString("dd/MM/yyyy") · @Session.EmpleadoNombre</p>
            </div>
            <div class="card-body p-4">
                <div class="row g-3">
                    <div class="col-md-6">
                        <div class="card border-0 bg-success bg-opacity-10 rounded-4 p-4 text-center">
                            <i class="bi bi-arrow-up-circle-fill text-success display-5 mb-2"></i>
                            <h6 class="text-muted fw-bold">VENTAS DEL DÍA</h6>
                            <p class="fs-2 fw-bold text-success mb-0">
                                RD$ @(_resumen?.TotalIngresos.ToString("N2") ?? "—")
                            </p>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card border-0 bg-danger bg-opacity-10 rounded-4 p-4 text-center">
                            <i class="bi bi-arrow-down-circle-fill text-danger display-5 mb-2"></i>
                            <h6 class="text-muted fw-bold">GASTOS DEL DÍA</h6>
                            <p class="fs-2 fw-bold text-danger mb-0">
                                RD$ @(_gastosDia.ToString("N2"))
                            </p>
                        </div>
                    </div>
                    <div class="col-12">
                        <div class="card border-0 bg-dark text-white rounded-4 p-4 text-center">
                            <h5 class="text-warning fw-bold">EFECTIVO ESPERADO EN CAJA</h5>
                            <p class="display-5 fw-bold mb-0">
                                RD$ @( ((_estado?.SaldoInicial ?? 0) + (_resumen?.TotalIngresos ?? 0) - _gastosDia).ToString("N2") )
                            </p>
                            <small class="text-white-50">Saldo inicial + ventas - gastos</small>
                        </div>
                    </div>
                </div>

                <hr class="my-4"/>
                <div class="row text-muted small">
                    <div class="col-6"><strong>Saldo inicial:</strong> RD$ @(_estado?.SaldoInicial.ToString("N2") ?? "—")</div>
                    <div class="col-6 text-end"><strong>Operaciones:</strong> @(_resumen?.CantidadOperaciones ?? 0)</div>
                    <div class="col-6"><strong>Estado caja:</strong> @(_estado?.Estado ?? "—")</div>
                    <div class="col-6 text-end"><strong>Cajero:</strong> @Session.EmpleadoNombre</div>
                </div>
            </div>
        </div>

        @if (!string.IsNullOrEmpty(_error))
        {
            <div class="alert alert-danger mt-3 small d-print-none">@_error</div>
        }
    }
</EmpleadoAuthGuard>

@code {
    private ResumenCajaDiarioDto? _resumen;
    private EstadoCajaDto?        _estado;
    private decimal               _gastosDia = 0;
    private bool _cargando = true;
    private bool _cerrando = false;
    private string _error  = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (Session.CajaId.HasValue)
        {
            _resumen    = await Api.GetResumenHoyAsync();
            _estado     = await Api.GetEstadoCajaAsync(Session.CajaId.Value);
            // Los gastos se leen desde EstadoCajaDto.SaldoFinal - SaldoInicial - ventas
            // Por ahora mostramos 0 hasta que el backend exponga ese dato
            _gastosDia  = 0;
        }
        _cargando = false;
    }

    private async Task Imprimir() => await JS.InvokeVoidAsync("window.print");

    private async Task CerrarCaja()
    {
        if (!Session.CajaId.HasValue || !Session.EmpleadoId.HasValue) return;
        _cerrando = true;
        _error    = string.Empty;

        decimal saldoFinal = (_estado?.SaldoInicial ?? 0) + (_resumen?.TotalIngresos ?? 0) - _gastosDia;
        bool ok = await Api.CerrarCajaAsync(Session.CajaId.Value, Session.EmpleadoId.Value, saldoFinal);

        if (ok)
        {
            Session.CerrarSesionEmpleado();
            Nav.NavigateTo("/empleado/apertura");
        }
        else
        {
            _error = "No se pudo cerrar la caja. Verifica la conexión e intenta nuevamente.";
        }

        _cerrando = false;
    }
}
```

- [ ] **Step 3: Compilar**

```bash
cd /c/PROYECTO/TransmisionesWeb && dotnet build 2>&1
```
Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
cd /c/PROYECTO
git add TransmisionesWeb/Components/Pages/Empleado/
git commit -m "feat(web): agregar FacturaEmpleado y CierreCaja"
```

---

## Task 15: Pruebas manuales + .http requests

**Files:**
- Modify: `TransmisionesIntegracion/TransmisionesIntegracion.http`

- [ ] **Step 1: Agregar requests de prueba al archivo .http**

Abrir `TransmisionesIntegracion/TransmisionesIntegracion.http` y agregar al final:

```http
### Estado del sistema
GET https://localhost:7001/api/integracion/estado

###

### Buscar cliente por documento (reemplazar con cédula real)
GET https://localhost:7001/api/integracion/clientes/buscar/001-0000000-1

###

### Obtener factura por ID (reemplazar con ID real)
GET https://localhost:7001/api/integracion/facturas/1

###

### Enviar factura por email
POST https://localhost:7001/api/integracion/facturas/1/enviar-email

###

### Registrar gasto en caja
POST https://localhost:7001/api/integracion/cajas/1/gasto
Content-Type: application/json

{
  "concepto": "Materiales de limpieza",
  "monto": 350.00
}
```

- [ ] **Step 2: Prueba manual — Portal del Cliente (golden path)**

1. Iniciar `TransmisionesIntegracion`: `cd /c/PROYECTO/TransmisionesIntegracion && dotnet run`
2. Iniciar `TransmisionesAPI`: `cd /c/PROYECTO/TransmisionesAPI && dotnet run`
3. Iniciar `TransmisionesWeb`: `cd /c/PROYECTO/TransmisionesWeb && dotnet run`
4. Abrir `https://localhost:[puerto-web]`
5. Ir a `/login` → ingresar cédula de un cliente existente
6. Verificar que redirige a `/cliente/ordenes`
7. Click en "Ver Factura" en una orden entregada
8. Verificar que muestra la factura completa con totales correctos
9. Click "Imprimir" — debe abrir diálogo de impresión del navegador
10. Click "Enviar al correo" — verificar que llega el email (requiere SMTP configurado)

- [ ] **Step 3: Prueba manual — Terminal de Caja (golden path)**

1. Ir a `/empleado/apertura`
2. Ingresar código y contraseña de un empleado existente, monto inicial (ej: 5000)
3. Verificar redirección a `/empleado/caja`
4. Buscar "aceite" en el buscador → seleccionar un producto
5. Cambiar cantidad a 2
6. Click "Registrar Gasto" → ingresar "Materiales" / 200 → "Registrar"
7. Click "Confirmar" → verificar redirección a `/empleado/factura/{id}`
8. Verificar factura con totales correctos
9. Click "Imprimir"
10. Ir a `/empleado/cierre` → verificar resumen → "Cerrar Caja" → redirige a apertura

- [ ] **Step 4: Prueba offline**

1. Detener `TransmisionesAPI` (Ctrl+C)
2. En la terminal de caja, verificar badge "🔴 Offline"
3. Crear una cotización y confirmar → debe encolarse en SQLite
4. Volver a iniciar `TransmisionesAPI`
5. Esperar 30 segundos → verificar que el badge vuelve a "🟢 En línea" y la transacción se sincronizó

- [ ] **Step 5: Commit final**

```bash
cd /c/PROYECTO
git add TransmisionesIntegracion/TransmisionesIntegracion.http
git commit -m "test: agregar requests .http para endpoints nuevos y guía de pruebas manuales"
```

---

## Checklist de self-review

- [x] Spec §3.1 autenticación cliente → Task 8 (Login.razor)
- [x] Spec §3.2 registro → Task 9 (Registro.razor)
- [x] Spec §3.2 órdenes, facturas lista → Task 10
- [x] Spec §3.2 factura detalle + email + imprimir → Task 11
- [x] Spec §4.1 login empleado + apertura → Task 12
- [x] Spec §4.2 terminal + búsqueda + gastos → Task 13
- [x] Spec §4.3 procesar orden → Task 13 (`ConfirmarOrden`)
- [x] Spec §4.4 factura empleado → Task 14
- [x] Spec §4.5 cierre + cuadre → Task 14 (CierreCaja.razor)
- [x] Spec §4.6 indicador offline → Task 7 (EstadoConexion.razor)
- [x] Spec §5 GET estado → Task 2
- [x] Spec §5 GET factura → Task 1 + Task 3
- [x] Spec §5 POST enviar-email → Task 3
- [x] Spec §5 POST gasto → Task 2
- [x] Spec §6 Polly + resiliencia → Task 6 (Program.cs)
- [x] Spec §7 MailKit + EmailService → Task 3
- [x] Spec §8 testing manual → Task 15
