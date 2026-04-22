using MailKit.Net.Smtp;
using MimeKit;

namespace TransmisionesIntegracion.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarFacturaAsync(string destinatario, string nombreCliente, FacturaEmailData datos)
    {
        var html = GenerarHtml(nombreCliente, datos);

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(
            _config["Email:NombreRemitente"] ?? "Transmisiones MAG",
            _config["Email:Usuario"]));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = $"Factura #{datos.NumeroFactura} - Transmisiones MAG";

        var body = new BodyBuilder { HtmlBody = html };
        mensaje.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Email:Host"] ?? "smtp.gmail.com",
            int.Parse(_config["Email:Port"] ?? "587"),
            MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _config["Email:Usuario"],
            _config["Email:Password"]);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Factura {Numero} enviada a {Correo}", datos.NumeroFactura, destinatario);
    }

    private static string GenerarHtml(string nombreCliente, FacturaEmailData f)
    {
        var filas = string.Join("", f.Detalles.Select(d =>
            $"<tr><td>{d.Descripcion}</td><td>{d.Cantidad}</td><td>RD$ {d.PrecioUnitario:N2}</td><td>RD$ {d.Subtotal:N2}</td></tr>"));

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><style>
          body { font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; }
          .header { background: #212529; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }
          .body { padding: 20px; border: 1px solid #dee2e6; }
          table { width: 100%; border-collapse: collapse; margin-top: 15px; }
          th { background: #f8f9fa; padding: 8px; text-align: left; border-bottom: 2px solid #dee2e6; }
          td { padding: 8px; border-bottom: 1px solid #dee2e6; }
          .total { font-size: 1.2em; font-weight: bold; color: #198754; }
        </style></head>
        <body>
          <div class="header">
            <h2>Transmisiones MAG</h2>
            <p style="margin:0">Tu comprobante de servicio</p>
          </div>
          <div class="body">
            <p>Estimado/a <strong>{{nombreCliente}}</strong>,</p>
            <p>Factura <strong>#{{f.NumeroFactura}}</strong> del {{f.FechaEmision:dd/MM/yyyy}}.</p>
            <table>
              <tr><th>Descripción</th><th>Cant.</th><th>Precio</th><th>Subtotal</th></tr>
              {{filas}}
            </table>
            <hr/>
            <p>Subtotal: <strong>RD$ {{f.Subtotal:N2}}</strong></p>
            <p>ITBIS (18%): <strong>RD$ {{f.ITBIS:N2}}</strong></p>
            <p class="total">TOTAL: RD$ {{f.Total:N2}}</p>
          </div>
        </body>
        </html>
        """;
    }
}

public class FacturaEmailData
{
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ITBIS { get; set; }
    public decimal Total { get; set; }
    public List<DetalleEmailData> Detalles { get; set; } = new();
}

public class DetalleEmailData
{
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
