using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TransmisionesIntegracion.Services;

public class EmailService
{
    private readonly string _host = "smtp.gmail.com";
    private readonly int _port = 587;
    private readonly string _user = "tu_correo@gmail.com"; // Reemplazar con credenciales reales
    private readonly string _pass = "tu_password_aplicacion"; // Reemplazar con credenciales reales

    public async Task EnviarFacturaRealAsync(string emailDestino, string numeroFactura, byte[] pdfAdjunto)
    {
        try
        {
            using var client = new SmtpClient(_host, _port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_user, _pass);

            var message = new MailMessage(_user, emailDestino)
            {
                Subject = $"Factura Transmisiones MAG - {numeroFactura}",
                Body = $"Estimado Cliente,\n\nAdjunto encontrará su factura oficial de Transmisiones MAG.\n\nGracias por su compra.",
                IsBodyHtml = false
            };

            // Adjuntamos el PDF de la factura
            using var ms = new System.IO.MemoryStream(pdfAdjunto);
            message.Attachments.Add(new Attachment(ms, $"Factura_{numeroFactura}.pdf", "application/pdf"));

            await client.SendMailAsync(message);
            Console.WriteLine($"[EMAIL] Factura {numeroFactura} enviada a {emailDestino}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
            throw;
        }
    }
}
